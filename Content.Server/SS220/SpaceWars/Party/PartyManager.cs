// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Content.Shared.SS220.CCVars;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    public event Action<PartyStatusChangedActionArgs>? PartyStatusChanged;
    public event Action<Party>? PartyUpdated;

    public event Action<PartyMember>? UserJoinedParty;
    public event Action<PartyMember>? UserLeavedParty;

    public IReadOnlyCollection<Party> Parties => _parties;
    private readonly HashSet<Party> _parties = [];

    private uint _nextPartyId = 0;

    private int _membersLimit;

    private PartySettings DefaultSetting => new()
    {
        MembersLimit = _membersLimit
    };

    public override void Initialize()
    {
        base.Initialize();

        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

        _cfg.OnValueChanged(CCVars220.PartyMembersLimit, value =>
        {
            _membersLimit = value;
            ResetSettings();
        }, true);

        SubscribeNetMessage<MsgCreatePartyRequest>(OnCreatePartyRequest);
        SubscribeNetMessage<MsgDisbandPartyRequest>(OnDisbandPartyRequest);
        SubscribeNetMessage<MsgLeavePartyRequest>(OnLeavePartyMessage);
        SubscribeNetMessage<MsgSetPartyHostRequest>(OnSetPartyHostRequest);
        SubscribeNetMessage<MsgKickFromPartyRequest>(OnKickFromPartyRequest);
        SubscribeNetMessage<MsgSetPartySettingsRequest>(OnSetSettingsRequest);

        InviteInitialize();
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        var oldConnected = e.OldStatus is SessionStatus.Connected or SessionStatus.InGame;
        var newConnected = e.NewStatus is SessionStatus.Connected or SessionStatus.InGame;

        if (oldConnected == newConnected)
            return;

        if (TryGetPartyByMember(e.Session, out var party))
            UpdateClientParty(party, [e.Session]);

        UpdateClient(e.Session, party);
    }

    private void OnCreatePartyRequest(MsgCreatePartyRequest message, ICommonSession sender)
    {
        CreateParty(sender, message.SettingsState);
    }

    private void OnDisbandPartyRequest(MsgDisbandPartyRequest message, ICommonSession sender)
    {
        if (!TryGetPartyByHost(sender, out var party))
            return;

        DisbandParty(party);
    }

    private void OnLeavePartyMessage(MsgLeavePartyRequest message, ICommonSession sender)
    {
        EnsureNotPartyMember(sender);
    }

    private void OnSetPartyHostRequest(MsgSetPartyHostRequest message, ICommonSession sender)
    {
        if (!TryGetPartyByHost(sender, out var party))
            return;

        if (!_playerManager.TryGetSessionById(message.UserId, out var session))
            return;

        SetHost(party, session);
    }

    private void OnKickFromPartyRequest(MsgKickFromPartyRequest message, ICommonSession sender)
    {
        if (!TryGetPartyByHost(sender, out var party))
            return;

        if (!_playerManager.TryGetSessionById(message.UserId, out var session))
            return;

        RemoveMember(party, session);
    }

    private void OnSetSettingsRequest(MsgSetPartySettingsRequest message, ICommonSession sender)
    {
        if (!TryGetPartyByHost(sender, out var party))
            return;

        SetPartySettings(party, message.State);
    }

    public bool CreateParty(ICommonSession host, PartySettingsState? settings = null, bool force = false)
    {
        return CreateParty(host, out _, settings, force);
    }

    public bool CreateParty(ICommonSession host, [NotNullWhen(true)] out Party? party, PartySettings? settings = null, bool force = false)
    {
        party = null;
        if (force)
            EnsureNotPartyMember(host);
        else if (IsAnyPartyMember(host))
            return false;

        party = new Party(GeneratePartyId(), host);
        _parties.Add(party);

        DebugTools.Assert(PartyExist(party));
        DebugTools.Assert(party.IsHost(host));

        settings ??= DefaultSetting;
        SetPartySettings(party, settings.Value, false);

        SetStatus(party, PartyStatus.Running, false);

        UserJoinedParty?.Invoke(party.Host);
        Dirty(party);

        return true;
    }

    public bool DisbandParty(Party party, bool updates = true)
    {
        if (!PartyExist(party))
            return false;

        SetStatus(party, PartyStatus.Disbanding, false);
        PartyUpdated?.Invoke(party);

        foreach (var member in party.Members)
        {
            if (party.IsHost(member.Session))
                continue;

            party.RemoveMember(member.Session);
            UpdateClientParty(member.Session, null);
        }

        foreach (var invite in GetInvitesByParty(party))
            DeleteInvite(invite);

        _parties.Remove(party);
        UpdateClientParty(party.Host.Session, null);

        party.Status = PartyStatus.Disbanded;
        DebugTools.Assert(!PartyExist(party));

        PartyUpdated?.Invoke(party);
        party.Dispose();

        return true;
    }

    public bool AddMember(Party party,
        ICommonSession session,
        PartyMemberRole role = PartyMemberRole.Member,
        bool force = false,
        bool ignoreLimit = false,
        bool updates = true,
        bool notify = true)
    {
        if (!PartyExist(party))
            return false;

        if (force)
            EnsureNotPartyMember(session);
        else if (IsAnyPartyMember(session))
            return false;

        if (!party.AddMember(session, role, out var member, ignoreLimit))
            return false;
        DebugTools.Assert(party.ContainsMember(session));

        if (notify)
        {
            var chatMessage = Loc.GetString("party-manager-user-join-party-message", ("username", session.Name));
            ChatMessageToParty(chatMessage, party, PartyChatMessageType.Info);

            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", chatMessage));
            foreach (var target in party.Members)
            {
                if (target == member)
                    continue;

                _chat.ChatMessageToOne(ChatChannel.Server, chatMessage, wrappedMessage, default, false, target.Session.Channel);
            }
        }

        UserJoinedParty?.Invoke(member);

        if (updates)
            Dirty(party);

        DeleteInvite(party, session);
        UpdateClientInvites(session);
        return true;
    }

    public bool RemoveMember(Party party, ICommonSession session, bool updates = true, bool notify = true)
    {
        if (!PartyExist(party))
            return false;

        if (!party.TryFindMember(session, out var member))
            return false;

        if (party.IsHost(member))
        {
            var possibleHosts = party.Members.ToList();
            possibleHosts.Remove(member);
            if (possibleHosts.Count <= 0)
                return DisbandParty(party);

            if (!SetHost(party, possibleHosts.First().Session, updates: false))
                return false;
        }

        if (!party.RemoveMember(member))
            return false;

        DebugTools.Assert(!party.ContainsMember(session));

        if (notify)
        {
            var chatMessage = Loc.GetString("party-manager-user-left-party-message", ("username", session.Name));
            ChatMessageToParty(chatMessage, party, PartyChatMessageType.Info);

            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", chatMessage));
            foreach (var target in party.Members)
            {
                if (target == member)
                    continue;

                _chat.ChatMessageToOne(ChatChannel.Server, chatMessage, wrappedMessage, default, false, target.Session.Channel);
            }
        }

        UserLeavedParty?.Invoke(member);
        UpdateClientInvites(session);

        if (updates)
        {
            UpdateClientParty(session, null);
            Dirty(party);
        }

        return true;
    }

    public bool SetHost(Party party, ICommonSession session, bool force = false, bool updates = true, bool notify = true)
    {
        if (!PartyExist(party))
            return false;

        if (party.IsHost(session))
            return true;

        var isMember = party.ContainsMember(session);
        if (!isMember)
        {
            if (force)
                EnsureNotPartyMember(session);
            else if (IsAnyPartyMember(session))
                return false;
        }

        var oldHost = party.Host;
        party.SetHost(session, ignoreLimit: force);
        DebugTools.Assert(!party.IsHost(oldHost.Session));
        DebugTools.Assert(party.IsHost(session));

        if (notify)
        {
            var chatMessage = Loc.GetString("party-manager-user-became-host-message", ("username", session.Name));
            ChatMessageToParty(chatMessage, party, PartyChatMessageType.Info);
        }

        if (!isMember)
            UserJoinedParty?.Invoke(party.Host);

        if (updates)
            Dirty(party);

        DeleteInvite(party, session);

        UpdateClientInvites(session);
        UpdateClientInvites(oldHost.Session);

        return true;
    }

    public bool SetStatus(Party party, PartyStatus newStatus, bool updates = true)
    {
        if (!PartyExist(party))
            return false;

        var oldStatus = party.Status;
        if (oldStatus == newStatus)
            return true;

        party.Status = newStatus;
        DebugTools.Assert(party.Status == newStatus);

        var args = new PartyStatusChangedActionArgs(party, oldStatus, newStatus);
        PartyStatusChanged?.Invoke(args);

        if (updates)
            Dirty(party);

        return true;
    }

    public void EnsureNotPartyMember(ICommonSession session, bool updates = true)
    {
        if (!TryGetPartyByMember(session, out var party))
            return;

        RemoveMember(party, session, updates);
        DebugTools.Assert(!IsAnyPartyMember(session));
    }

    public bool IsAnyPartyMember(ICommonSession session)
    {
        return TryGetPartyByMember(session, out _);
    }

    public bool PartyExist(Party? party)
    {
        if (party is null)
            return false;

        var exist = _parties.Contains(party);
        if (exist)
            DebugTools.Assert(party.Status is not PartyStatus.Disbanded);

        return exist;
    }

    public bool TryGetPartyById(uint id, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyById(id);
        return party != null;
    }

    public bool TryGetPartyByHost(ICommonSession host, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyByHost(host);
        return party != null;
    }

    public bool TryGetPartyByMember(ICommonSession member, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyByMember(member);
        return party != null;
    }

    public Party? GetPartyById(uint id)
    {
        var result = _parties.Where(p => p.Id == id);
        var count = result.Count();
        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        var party = result.First();

        DebugTools.Assert(PartyExist(party));
        return party;
    }

    public Party? GetPartyByHost(ICommonSession session)
    {
        var result = _parties.Where(p => p.IsHost(session));
        var count = result.Count();
        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        var party = result.First();

        DebugTools.Assert(PartyExist(party));
        return party;
    }

    public Party? GetPartyByMember(ICommonSession session)
    {
        var result = _parties.Where(p => p.ContainsMember(session));
        var count = result.Count();
        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        var party = result.First();

        DebugTools.Assert(PartyExist(party));
        return party;
    }

    public IEnumerable<CompletionOption> GetPartiesCompletionOptions()
    {
        var result = new List<CompletionOption>();
        foreach (var party in _parties)
        {
            var hint = Loc.GetString("party-manager-party-completion-option", ("host", party.Host.Name));
            result.Add(new CompletionOption(party.Id.ToString(), hint));
        }

        return result;
    }

    public void Dirty(Party party)
    {
        UpdateClientParty(party);
        PartyUpdated?.Invoke(party);
    }

    private void UpdateClient(ICommonSession session)
    {
        UpdateClient(session, GetPartyByMember(session));
    }

    private void UpdateClient(ICommonSession session, Party? curParty)
    {
        UpdateClientParty(session, curParty);
        UpdateClientInvites(session);
    }

    private void UpdateClientParty(Party party, List<ICommonSession>? blacklist = null)
    {
        foreach (var member in party.Members)
        {
            if (blacklist != null && blacklist.Contains(member.Session))
                continue;

            UpdateClientParty(member.Session, party);
        }
    }

    private void UpdateClientParty(ICommonSession session)
    {
        UpdateClientParty(session, GetPartyByMember(session));
    }

    private void UpdateClientParty(ICommonSession session, Party? party)
    {
        DebugTools.Assert(party == GetPartyByMember(session));

        var ev = new MsgUpdateClientParty(party?.GetState());
        SendNetMessage(ev, session);
    }

    private uint GeneratePartyId()
    {
        return _nextPartyId++;
    }

    private void SendNetMessage(PartyMessage message)
    {
        var msg = new PartyNetMessage
        {
            Message = message
        };

        _net.ServerSendToAll(msg);
    }

    private void SendNetMessage(PartyMessage message, ICommonSession session)
    {
        var msg = new PartyNetMessage
        {
            Message = message
        };

        _net.ServerSendMessage(msg, session.Channel);
    }

    #region Settings
    private void ResetSettings()
    {
        foreach (var party in _parties)
            ResetSettings(party);
    }

    private void ResetSettings(Party party)
    {
        SetPartySettings(party, party.Settings);
    }

    public void SetPartySettings(Party party, PartySettings settings, bool updates = true)
    {
        settings.MembersLimit = Math.Clamp(settings.MembersLimit, 0, _membersLimit);

        party.Settings = settings;

        if (updates)
            Dirty(party);
    }
    #endregion
}
