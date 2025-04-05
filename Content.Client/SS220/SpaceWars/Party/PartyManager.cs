using Content.Client.SS220.SpaceWars.Party.UI.Tabs;
using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    public event Action<PartyData?>? OnPartyDataUpdated;

    public PartyData? CurrentParty => _currentParty;
    private PartyData? _currentParty;

    public PartyMenuEui? PartyMenuEui => _partyMenuEui;
    private PartyMenuEui? _partyMenuEui;

    public void SetPartyData(PartyData? currentParty)
    {
        _currentParty = currentParty;
        OnPartyDataUpdated?.Invoke(currentParty);
    }

    #region PartyMenuUI
    public void SetPartyMenuEui(PartyMenuEui? partyMenuEui)
    {
        _partyMenuEui = partyMenuEui;
    }
    #endregion
}
