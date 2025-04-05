using Content.Server.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public interface IPartyManager : ISharedPartyManager
{
    event Action<PartyData>? OnPartyDataUpdated;
    event Action<PartyData>? OnPartyDisbanded;

    [Access(Other = AccessPermissions.Read)]
    List<PartyData> Parties { get; }

    Dictionary<ICommonSession, PartyMenuEui> OpenedMenu { get; }

    bool TryCreateParty(NetUserId leader, [NotNullWhen(false)] out string? reason);

    /// <summary>
    ///     Creates a new party with the <paramref name="leader"/>
    /// </summary>
    /// <exception cref="ArgumentException"> <paramref name="leader"/> must not be present at another party </exception>
    PartyData? CreateParty(NetUserId leader);

    void DisbandParty(PartyData party);

    PartyData? GetPartyByLeader(NetUserId leader);

    PartyData? GetPartyByMember(NetUserId member);

    bool TryAddPlayerToParty(NetUserId member, PartyData party, [NotNullWhen(false)] out string? reason);

    /// <summary>
    ///     Adds a <paramref name="member"/> to the <paramref name="party"/>
    /// </summary>
    /// <exception cref="ArgumentException"> <paramref name="member"/> must not be present at another party </exception>
    void AddPlayerToParty(NetUserId member, PartyData party);

    void RemovePlayerFromParty(NetUserId member, PartyData party);

    #region PartyMenuUI
    void OpenPartyMenu(ICommonSession session);

    void ClosePartyMenu(ICommonSession session);
    #endregion
}
