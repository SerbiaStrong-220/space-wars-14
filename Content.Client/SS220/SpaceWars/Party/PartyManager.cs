// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    public event Action<(Party? Old, Party? New)>? LocalPartyChanged;
    public event Action<Party>? LocalPartyUpdated;

    public PartyUIController UIController => _ui.GetUIController<PartyUIController>();

    public Party? LocalParty { get; private set; }

    public PartyEventsHandler LocalPartyEvents { get; private set; } = new();

    public PartyMember? LocalMember
    {
        get
        {
            if (LocalParty is not { } party)
                return null;

            if (_player.LocalUser is not { } userId)
                return null;

            var member = party.FindMember(userId);
            DebugTools.AssertNotNull(member);
            return member;
        }
    }

    public bool IsLocalPartyHost
    {
        get
        {
            if (LocalParty is not { } party)
                return false;

            if (_player.LocalUser is not { } userId)
                return false;

            return party.IsHost(userId);
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetMessage<MsgUpdateClientParty>(OnUpdatePartyMessage);

        InviteInitialize();
        ChatInitialize();
    }

    private void OnUpdatePartyMessage(MsgUpdateClientParty message)
    {
        if (message.State is not { } state)
        {
            var oldParty = LocalParty;
            if (oldParty != null)
                oldParty.EventsHandler = null;

            LocalParty = null;

            LocalPartyChanged?.Invoke((oldParty, LocalParty));
            UIController.RefreshWindow();

            return;
        }

        if (LocalParty is not { } party || party.Id != state.Id)
        {
            var oldParty = LocalParty;
            LocalParty = new(state)
            {
                EventsHandler = LocalPartyEvents
            };

            LocalPartyChanged?.Invoke((LocalParty, oldParty));
            UIController.RefreshWindow();

            return;
        }

        party.HandleState(state);
        LocalPartyUpdated?.Invoke(LocalParty);
        UIController.RefreshWindow();
    }

    public void CreatePartyRequest(PartySettingsState? settings = null)
    {
        var msg = new MsgCreatePartyRequest(settings);
        SendNetMessage(msg);
    }

    public void DisbandPartyRequest()
    {
        var msg = new MsgDisbandPartyRequest();
        SendNetMessage(msg);
    }

    public void LeavePartyRequest()
    {
        var msg = new MsgLeavePartyRequest();
        SendNetMessage(msg);
    }

    public void SetPartyHostRequest(NetUserId userId)
    {
        var msg = new MsgSetPartyHostRequest(userId);
        SendNetMessage(msg);
    }

    public void KickFromPartyRequest(NetUserId userId)
    {
        var msg = new MsgKickFromPartyRequest(userId);
        SendNetMessage(msg);
    }

    public void SetSettingsRequest(PartySettingsState settingsState)
    {
        var msg = new MsgSetPartySettingsRequest(settingsState);
        SendNetMessage(msg);
    }

    public void SendInviteRequest(string username)
    {
        var msg = new MsgInviteUserRequest(username);
        SendNetMessage(msg);
    }

    private void SendNetMessage(PartyMessage message)
    {
        if (_player.LocalSession is { } session)
            message.Sender = session.UserId;

        var msg = new PartyNetMessage
        {
            Message = message,
        };

        _net.ClientSendMessage(msg);
    }

    private static void HandlePartyState(Party party, PartyState state)
    {
        DebugTools.Assert(party.Id == state.Id);

        party.Status = state.Status;
    }
}
