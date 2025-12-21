// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Client.Lobby.UI;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Shared.SS220.Input;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Timing;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.SS220.SpaceWars.Party.UI;

[UsedImplicitly]
public sealed class PartyUIController : UIController, IOnStateChanged<GameplayState>, IOnStateChanged<LobbyState>
{
    [Dependency] private readonly IPartyManager _party = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private PartyWindow? _window;
    private MenuButton? GamePartyButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.PartyButton;
    private Button? LobbyPartyButton => (UIManager.ActiveScreen as LobbyGui)?.PartyButton;

    public static readonly Color DefaultBackgroundColor = new(60, 60, 60);
    public static readonly Color DefaultInnerBackgroundColor = new(32, 32, 40);

    private static readonly TimeSpan ClosedWindowLifeTime = TimeSpan.FromSeconds(15);
    private TimeSpan _windowDisposeTime = TimeSpan.Zero;

    private bool _bindsRegistered;
    private bool _hasUnreadInfo;

    public override void Initialize()
    {
        base.Initialize();

        _party.LocalPartyChanged += OnLocalPartyChanged;

        _party.LocalPartyEvents.HostChanged += _ => UnreadInfoReceived();

        _party.LocalPartyEvents.MemberAdded += _ => UnreadInfoReceived();
        _party.LocalPartyEvents.MemberRemoved += _ => UnreadInfoReceived();

        _party.LocalPartyEvents.InviteAdded += _ => _window?.MainTab.LocalPartyInvitesWindow?.Refresh();
        _party.LocalPartyEvents.InviteRemoved += _ => _window?.MainTab.LocalPartyInvitesWindow?.Refresh();
        _party.LocalPartyEvents.InviteUpdated += OnLocalPartyInviteUpdated;

        _party.ReceivedInviteAdded += OnReceivedInviteAdded;
        _party.ReceivedInviteRemoved += OnReceivedInviteRemoved;
        _party.ReceivedInviteUpdated += OnReceivedInviteUpdated;

        _party.ChatMessageReceived += OnChatMessageReceived;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_window != null && !_window.IsOpen && _gameTiming.CurTime >= _windowDisposeTime)
            _window = null;
    }

    private void OnLocalPartyChanged((Party? Old, Party? New) tulpe)
    {
        UnreadInfoReceived();
        _window?.Refresh();
    }

    private void OnLocalPartyInviteUpdated(PartyInvite invite)
    {
        if (_window?.MainTab.LocalPartyInvitesWindow is not { } localPartyInvitesWindow)
            return;

        if (localPartyInvitesWindow.TryGetEntry(invite, out var entry))
            entry.Refresh();
        else
            localPartyInvitesWindow.Refresh();
    }

    private void OnReceivedInviteAdded(PartyInvite invite)
    {
        _window?.ReceivedInvitesTab.AddInvite(invite);
        UnreadInfoReceived();
    }

    private void OnReceivedInviteRemoved(PartyInvite invite)
    {
        _window?.ReceivedInvitesTab.RemoveInvite(invite);
        UnreadInfoReceived();
    }

    private void OnReceivedInviteUpdated(PartyInvite invite)
    {
        if (_window == null)
            return;

        if (_window.ReceivedInvitesTab.TryGetEntry(invite, out var entry))
            entry.Refresh();
        else
            _window.ReceivedInvitesTab.Refresh();
    }

    private void OnChatMessageReceived(string message)
    {
        _window?.MainTab.AddChatMessage(message);
        UnreadInfoReceived();
    }

    public void LoadButton()
    {
        if (GamePartyButton != null)
            GamePartyButton.OnPressed += PartyButtonPressed;

        if (LobbyPartyButton != null)
            LobbyPartyButton.OnPressed += PartyButtonPressed;
    }

    public void UnloadButton()
    {
        if (GamePartyButton != null)
            GamePartyButton.OnPressed -= PartyButtonPressed;

        if (LobbyPartyButton != null)
            LobbyPartyButton.OnPressed -= PartyButtonPressed;
    }

    public void OpenWindow()
    {
        EnsureWindow().OpenCentered();
    }

    public void CloseWindow()
    {
        _window?.Close();
    }

    public void ToggleWindow()
    {
        if (_window != null && _window.IsOpen)
            CloseWindow();
        else
            OpenWindow();
    }

    public void RefreshWindow()
    {
        _window?.Refresh();
    }

    public void OnStateEntered(GameplayState state)
    {
        if (GamePartyButton != null)
        {
            GamePartyButton.OnPressed -= PartyButtonPressed;
            GamePartyButton.OnPressed += PartyButtonPressed;
        }

        if (_window != null &&
            _window.IsOpen)
            ActivateButton();
        else
            DeactivateButton();

        EnsureBinds();
    }

    public void OnStateExited(GameplayState state)
    {
        if (GamePartyButton != null)
            GamePartyButton.OnPressed -= PartyButtonPressed;
    }

    public void OnStateEntered(LobbyState state)
    {
        if (LobbyPartyButton != null)
        {
            LobbyPartyButton.OnPressed -= PartyButtonPressed;
            LobbyPartyButton.OnPressed += PartyButtonPressed;
        }

        if (_window != null &&
            _window.IsOpen)
            ActivateButton();
        else
            DeactivateButton();

        EnsureBinds();
    }

    public void OnStateExited(LobbyState state)
    {
        if (LobbyPartyButton != null)
            LobbyPartyButton.OnPressed -= PartyButtonPressed;
    }

    private PartyWindow EnsureWindow()
    {
        if (_window != null)
            return _window;

        _window = new PartyWindow();
        _window.OnClose += OnWindowClosed;
        _window.OnOpen += ActivateButton;

        _windowDisposeTime = _gameTiming.CurTime + ClosedWindowLifeTime;
        return _window;
    }

    private void OnWindowClosed()
    {
        DeactivateButton();
        _windowDisposeTime = _gameTiming.CurTime + ClosedWindowLifeTime;
    }

    private void ClearWindow()
    {
        _window?.Close();
        _window = null;
    }

    private void EnsureBinds()
    {
        if (_bindsRegistered)
            return;

        CommandBinds.Builder
            .Bind(KeyFunctions220.TogglePartyWindow,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<PartyUIController>();

        _bindsRegistered = true;
    }

    private void ClearBinds()
    {
        CommandBinds.Unregister<PartyUIController>();
        _bindsRegistered = false;
    }

    private void DeactivateButton()
    {
        if (GamePartyButton != null)
            GamePartyButton.Pressed = false;

        if (LobbyPartyButton != null)
            LobbyPartyButton.Pressed = false;
    }

    private void ActivateButton()
    {
        if (GamePartyButton != null)
            GamePartyButton.Pressed = true;

        if (LobbyPartyButton != null)
            LobbyPartyButton.Pressed = true;

        UnreadInfoRead();
    }

    private void PartyButtonPressed(ButtonEventArgs args)
    {
        if (_window == null)
            return;

        ToggleWindow();
    }

    private void UnreadInfoReceived()
    {
        if (_hasUnreadInfo || _window?.IsOpen is true)
            return;

        GamePartyButton?.StyleClasses.Add(MenuButton.StyleClassRedTopButton);
        LobbyPartyButton?.StyleClasses.Add(StyleNano.StyleClassButtonColorRed);
        _hasUnreadInfo = true;
    }

    private void UnreadInfoRead()
    {
        if (!_hasUnreadInfo)
            return;

        GamePartyButton?.StyleClasses.Remove(MenuButton.StyleClassRedTopButton);
        LobbyPartyButton?.StyleClasses.Remove(StyleNano.StyleClassButtonColorRed);
        _hasUnreadInfo = false;
    }
}
