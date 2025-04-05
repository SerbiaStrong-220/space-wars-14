using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Server.SS220.SpaceWars.Party.UI;

public sealed class PartyMenuEui : BaseEui
{
    [Dependency] private readonly IPartyManager _partyManager = default!;

    public PartyMenuEui()
    {
        IoCManager.InjectDependencies(this);
    }

    public override void Closed()
    {
        base.Closed();
        _partyManager.ClosePartyMenu(Player);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        PartyData? party = null;
        switch (msg)
        {
            case PartyMenuEuiMsg.CreateParty:
                _partyManager.CreateParty(Player.UserId);
                break;

            case PartyMenuEuiMsg.DisbandParty:
                party = _partyManager.GetPartyByLeader(Player.UserId);
                if (party != null)
                    _partyManager.DisbandParty(party);
                break;

            case PartyMenuEuiMsg.LeaveParty:
                party = _partyManager.GetPartyByMember(Player.UserId);
                if (party != null)
                    _partyManager.RemovePlayerFromParty(Player.UserId, party);
                break;

            default:
                break;
        }
    }
}
