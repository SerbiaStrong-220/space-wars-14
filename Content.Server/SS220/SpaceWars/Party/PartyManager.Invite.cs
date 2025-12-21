// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.Chat;
using Content.Shared.SS220.CCVars;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<PartyInviteStatusChangedActionArgs>? PartyInviteStatusChanged;

    public IReadOnlyList<PartyInvite> Invites => [.. _invites];
    private readonly HashSet<PartyInvite> _invites = [];
    private readonly HashSet<ICommonSession> _doesNotReceiveInvites = [];

    private uint _nextInviteId = 0;
    private int _invitesLimit = 0;

    public static readonly TimeSpan DeleteAfterDenyDelay = TimeSpan.FromSeconds(5);

    public void InviteInitialize()
    {
        SubscribeNetMessage<MsgInviteUserRequest>(OnInviteInPartyRequest);
        SubscribeNetMessage<MsgAcceptInviteRequest>(OnAcceptInvite);
        SubscribeNetMessage<MsgDenyInviteRequest>(OnDenyInvite);
        SubscribeNetMessage<MsgDeleteInviteRequest>(OnDeleteInvite);
        SubscribeNetMessage<MsgSetReceiveInvitesStatus>(OnSetReceiveInvitesStatus);

        _cfg.OnValueChanged(CCVars220.PartyInvitesLimit, value => _invitesLimit = value, true);
    }

    private void OnInviteInPartyRequest(MsgInviteUserRequest message, ICommonSession sender)
    {
        var success = TryInvite(out var responceMessage);

        var responce = new MsgInviteUserResponce
        {
            Id = message.Id,
            Success = success,
            Text = responceMessage
        };

        SendNetMessage(responce, sender);

        bool TryInvite(out string responceMessage)
        {
            if (!TryGetPartyByHost(sender, out var party))
            {
                responceMessage = Loc.GetString("party-invite-request-hosted-party-not-found");
                return false;
            }

            if (!_playerManager.TryGetSessionByUsername(message.Username, out var receiver))
            {
                responceMessage = Loc.GetString("party-invite-request-user-not-found", ("username", message.Username));
                return false;
            }

            if (!CreateInvite(party, receiver, out var checkoutResult, out _))
            {
                responceMessage = checkoutResult switch
                {
                    PartyInviteCheckoutResult.PartyNotExist => Loc.GetString("party-invite-request-party-not-exist", ("partyId", party.Id)),
                    PartyInviteCheckoutResult.AlreadyMember => Loc.GetString("party-invite-request-receiver-already-member", ("username", receiver.Name)),
                    PartyInviteCheckoutResult.InvitesLimitReached => Loc.GetString("party-invite-request-invites-limit-reached"),
                    PartyInviteCheckoutResult.MembersLimitReached => Loc.GetString("party-invite-request-members-limit-reached"),
                    PartyInviteCheckoutResult.AlreadyInvited => Loc.GetString("party-invite-request-already-invited", ("username", receiver.Name)),
                    PartyInviteCheckoutResult.DoesNotReseive => Loc.GetString("party-invite-request-does-not-receive", ("username", receiver.Name)),
                    _ => Loc.GetString("party-invite-request-other-reason", ("username", receiver.Name))
                };
                return false;
            }

            responceMessage = string.Empty;
            return true;
        }
    }

    private void OnAcceptInvite(MsgAcceptInviteRequest message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (sender != invite.Target)
            return;

        AcceptInvite(invite);
    }

    private void OnDenyInvite(MsgDenyInviteRequest message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (sender != invite.Target)
            return;

        DenyInvite(invite);
    }

    private void OnDeleteInvite(MsgDeleteInviteRequest message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (invite.Party.Host.Session != sender)
            return;

        DeleteInvite(invite);
    }

    private void OnSetReceiveInvitesStatus(MsgSetReceiveInvitesStatus message, ICommonSession sender)
    {
        if (message.ReceiveInvites)
            _doesNotReceiveInvites.Remove(sender);
        else
            _doesNotReceiveInvites.Add(sender);
    }

    public bool AcceptInvite(PartyInvite invite, bool ignoreLimit = false)
    {
        var party = invite.Party;
        if (!AddMember(party, invite.Target, PartyMemberRole.Member, force: true, ignoreLimit))
            return false;

        SetInviteStatus(invite, PartyInviteStatus.Accepted, updates: false);

        DebugTools.Assert(party.ContainsMember(invite.Target));
        DeleteInvite(invite);
        return true;
    }

    public void DenyInvite(PartyInvite invite)
    {
        SetInviteStatus(invite, PartyInviteStatus.Denied);
        Timer.Spawn(DeleteAfterDenyDelay, () => DeleteInvite(invite));
    }

    public bool CreateInvite(Party party, ICommonSession target, [NotNullWhen(true)] out PartyInvite? invite)
    {
        return CreateInvite(party, target, out _, out invite);
    }

    public bool CreateInvite(Party party, ICommonSession target, out PartyInviteCheckoutResult result, [NotNullWhen(true)] out PartyInvite? invite)
    {
        invite = null;
        if (!InviteAvailableCheckout(party, target, out result))
            return false;

        invite = new PartyInvite(GenerateInviteId(), party, target);
        _invites.Add(invite);
        party.AddInvite(invite);

        // Notify dependent systems about the invite creation before it is sent
        SetInviteStatus(invite, PartyInviteStatus.Created, updates: false);
        Dirty(party);

        var msg = new MsgHandleReceivedPartyInviteState(invite.GetState());
        SendNetMessage(msg, invite.Target);

        SetInviteStatus(invite, PartyInviteStatus.Sended);

        NotifyHost();
        NotifyTarget();

        return true;

        void NotifyHost()
        {
            var inGameChatMessage = Loc.GetString("party-manager-invite-sended-ingame-chat-message", ("username", target.Name));
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", inGameChatMessage));
            _chat.ChatMessageToOne(ChatChannel.Server, inGameChatMessage, wrappedMessage, default, false, party.Host.Session.Channel);
        }

        void NotifyTarget()
        {
            var inGameChatMessage = Loc.GetString("party-manager-invite-received-ingame-chat-message", ("username", party.Host.Name));
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", inGameChatMessage));
            _chat.ChatMessageToOne(ChatChannel.Server, inGameChatMessage, wrappedMessage, default, false, target.Channel);
        }
    }

    public bool DeleteInvite(uint inviteId)
    {
        if (!TryGetInvite(inviteId, out var invite))
            return false;

        return DeleteInvite(invite);
    }

    public bool DeleteInvite(Party party, ICommonSession receiver)
    {
        if (!TryGetInvite(party, receiver, out var invite))
            return false;

        return DeleteInvite(invite);
    }

    public bool DeleteInvite(PartyInvite invite)
    {
        if (!InviteExist(invite))
            return false;

        _invites.Remove(invite);

        var party = invite.Party;
        party.RemoveInvite(invite);
        Dirty(party);

        SetInviteStatus(invite, PartyInviteStatus.Deleted, false);
        var msg = new MsgHandleReceivedPartyInviteState(invite.GetState());
        SendNetMessage(msg, invite.Target);

        return true;
    }

    public bool InviteAvailable(Party party, ICommonSession target)
    {
        return InviteAvailableCheckout(party, target, out _);
    }

    public bool InviteAvailableCheckout(Party party, ICommonSession target, out PartyInviteCheckoutResult result, bool ingoreMembersLimit = false)
    {
        if (!PartyExist(party))
        {
            result = PartyInviteCheckoutResult.PartyNotExist;
            return false;
        }

        if (party.ContainsMember(target))
        {
            result = PartyInviteCheckoutResult.AlreadyMember;
            return false;
        }

        var existedInvites = GetInvitesByParty(party);
        if (existedInvites.Any(i => i.Target == target))
        {
            result = PartyInviteCheckoutResult.AlreadyInvited;
            return false;
        }

        if (existedInvites.Count() > _invitesLimit)
        {
            result = PartyInviteCheckoutResult.InvitesLimitReached;
            return false;
        }

        if (!ingoreMembersLimit && party.MembersLimitReached)
        {
            result = PartyInviteCheckoutResult.MembersLimitReached;
            return false;
        }

        if (_doesNotReceiveInvites.Contains(target))
        {
            result = PartyInviteCheckoutResult.DoesNotReseive;
            return false;
        }

        result = PartyInviteCheckoutResult.Available;
        return true;
    }

    public bool TryGetInvite(uint inviteId, [NotNullWhen(true)] out PartyInvite? invite)
    {
        invite = GetInvite(inviteId);
        return invite != null;
    }

    public bool TryGetInvite(Party party, ICommonSession receiver, [NotNullWhen(true)] out PartyInvite? invite)
    {
        invite = GetInvite(party, receiver);
        return invite != null;
    }

    public PartyInvite? GetInvite(uint inviteId)
    {
        return _invites.FirstOrDefault(i => i.Id == inviteId);
    }

    public PartyInvite? GetInvite(Party party, ICommonSession target)
    {
        return _invites.FirstOrDefault(i => i.Party == party && i.Target == target);
    }

    public IEnumerable<PartyInvite> GetInvitesByParty(Party party)
    {
        return party.Invites;
    }

    public IEnumerable<PartyInvite> GetInvitesByTarget(ICommonSession target)
    {
        return _invites.Where(i => i.Target == target);
    }

    public void SetInviteStatus(PartyInvite invite, PartyInviteStatus status, bool updates = true)
    {
        var oldStatus = invite.Status;

        if (oldStatus == status)
            return;

        invite.Status = status;
        PartyInviteStatusChanged?.Invoke(new PartyInviteStatusChangedActionArgs(invite.Id, oldStatus, status));

        if (updates)
        {
            UpdateClientInviteState(invite);
        }
    }

    public bool InviteExist(PartyInvite? invite)
    {
        if (invite is null)
            return false;

        return _invites.Contains(invite);
    }

    private uint GenerateInviteId()
    {
        return _nextInviteId++;
    }

    private void UpdateClientInvites(ICommonSession session)
    {
        var states = GetInvitesByTarget(session).Select(i => i.GetState()).ToList();

        if (TryGetPartyByHost(session, out var party))
            states.AddRange(GetInvitesByParty(party).Select(i => i.GetState()));

        var msg = new MsgUpdateReceivedPartyInvitesList(states);
        SendNetMessage(msg, session);
    }

    private void UpdateClientInviteState(PartyInvite invite)
    {
        Dirty(invite.Party);
        UpdateClientInviteState(invite.Target, invite);
    }

    private void UpdateClientInviteState(ICommonSession session, PartyInvite invite)
    {
        DebugTools.Assert(invite.Party.Host.Session == session || invite.Target == session);

        var msg = new MsgHandleReceivedPartyInviteState(invite.GetState());
        SendNetMessage(msg, session);
    }
}
