using Content.Client.SS220.SpaceWars.Party.Systems;
using Content.Client.SS220.SpaceWars.Party.UI.Tabs;
using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;

namespace Content.Client.SS220.SpaceWars.Party;

public interface IPartyManager : ISharedPartyManager
{
    event Action<PartyData?>? OnPartyDataUpdated;

    PartyMenuEui? PartyMenuEui { get; }

    PartyData? CurrentParty { get; }

    [Access(typeof(SharedPartySystem))]
    void SetPartyData(PartyData? currentParty);

    #region PartyMenuUI
    [Access(typeof(PartyMenuEui))]
    void SetPartyMenuEui(PartyMenuEui? partyMenuEui);
    #endregion
}
