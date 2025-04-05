using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party.UI.Tabs;

public sealed class PartyMenuEui : BaseEui
{
    [Dependency] private readonly IPartyManager _partyManager = default!;

    private readonly PartyMenu _window;

    public PartyMenuEui()
    {
        IoCManager.InjectDependencies(this);

        _partyManager.SetPartyMenuEui(this);
        _window = new PartyMenu();
    }

    public override void Opened()
    {
        base.Opened();

        _partyManager.SetPartyMenuEui(this);
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        _window.Close();
    }

    public void UpdateWindow()
    {
        _window.Update();
    }

    public void CreatePartyRequest()
    {
        var msg = new PartyMenuEuiMsg.CreateParty();
        SendMessage(msg);
    }

    public void DisbandPartyRequest()
    {
        var msg = new PartyMenuEuiMsg.DisbandParty();
        SendMessage(msg);
    }

    public void LeavePartyRequest()
    {
        var msg = new PartyMenuEuiMsg.LeaveParty();
        SendMessage(msg);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
    }
}
