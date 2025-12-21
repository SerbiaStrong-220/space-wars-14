// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Console;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action<PartyStatusChangedActionArgs>? PartyStatusChanged;
    event Action<Party>? PartyUpdated;

    event Action<PartyMember>? UserJoinedParty;
    event Action<PartyMember>? UserLeavedParty;

    IReadOnlyCollection<Party> Parties { get; }

    /// <inheritdoc cref="CreateParty(ICommonSession, out Party?, PartySettings?, bool)"/>
    bool CreateParty(ICommonSession host, PartySettingsState? settings = null, bool force = false);

    /// <summary>
    /// Creates a new party with the specified <paramref name="host"/> and <paramref name="settings"/>
    /// </summary>
    /// <param name="force">Should the <paramref name="host"/> be removed from other party or not</param>
    /// <returns><see langword="true"/> if a party was created</returns>
    bool CreateParty(ICommonSession host, [NotNullWhen(true)] out Party? party, PartySettings? settings = null, bool force = false);

    /// <summary>
    /// Disbandes the <paramref name="party"/>
    /// </summary>
    /// <param name="updates">Should party update events be triggered or not</param>
    /// <returns><see langword="true"/> if the party was disbanded</returns>
    bool DisbandParty(Party party, bool updates = true);

    /// <summary>
    /// Adds the <paramref name="session"/> in the <paramref name="party"/>
    /// </summary>
    /// <param name="force">Should the <paramref name="session"/> be removed from other party or not</param>
    /// <param name="ignoreLimit">Should the party invites limit be ignored or not</param>
    /// <param name="updates">Should party update events be triggered or not</param>
    /// <param name="notify">Should the notify in chat be sended or not</param>
    /// <returns><see langword="true"/> if the <paramref name="session"/> was added in the <paramref name="party"/></returns>
    bool AddMember(Party party,
        ICommonSession session,
        PartyMemberRole role = PartyMemberRole.Member,
        bool force = false,
        bool ignoreLimit = false,
        bool updates = true,
        bool notify = true);

    /// <summary>
    /// Removes the <paramref name="session"/> from the <paramref name="party"/>
    /// </summary>
    /// <remarks>
    /// Cannot remove member with the <see cref="PartyMemberRole.Host"/> role.
    /// Use the <see cref="SetHost(Party, ICommonSession, bool, bool)"/> function to set a new party host and then remove this member
    /// </remarks>
    /// <param name="updates">Should party update events be triggered or not</param>
    /// <param name="notify">Should the notify in chat be sended or not</param>
    /// <returns><see langword="true"/> if the <paramref name="session"/> was removed from the <paramref name="party"/></returns>
    bool RemoveMember(Party party, ICommonSession session, bool updates = true, bool notify = true);

    /// <summary>
    /// Sets a new host of the <paramref name="party"/>
    /// </summary>
    /// <param name="force">Should the <paramref name="session"/> be removed from other party or not</param>
    /// <param name="updates">Should party update events be triggered or not</param>
    /// <returns><see langword="true"/> if the <paramref name="session"/> was set as the host in the <paramref name="party"/></returns>
    bool SetHost(Party party, ICommonSession session, bool force = false, bool updates = true, bool notify = true);

    /// <summary>
    /// Sets a new status of the <paramref name="party"/>
    /// </summary>
    /// <param name="updates">Should party update events be triggered or not</param>
    /// <returns><see langword="true"/> if the <paramref name="newStatus"/> was set in the <paramref name="party"/></returns>
    bool SetStatus(Party party, PartyStatus newStatus, bool updates = true);

    /// <summary>
    /// Ensures that the <paramref name="session"/> will not be a member of any party
    /// </summary>
    /// <remarks>
    /// If the <paramref name="session"/> is a member of some party, then the <paramref name="session"/> will be removed from this party.
    /// If the <paramref name="session"/> is a host of some party, then this party will disbanded
    /// </remarks>
    /// <param name="updates">Should party update events be triggered or not</param>
    void EnsureNotPartyMember(ICommonSession session, bool updates = true);

    /// <summary>
    /// Is the <paramref name="session"/> is a member of any party
    /// </summary>
    bool IsAnyPartyMember(ICommonSession session);

    /// <summary>
    /// Is the <paramref name="party"/> exist
    /// </summary>
    bool PartyExist(Party? party);

    /// <summary>
    /// Tries to get the party by specified id
    /// </summary>
    bool TryGetPartyById(uint id, [NotNullWhen(true)] out Party? party);

    /// <summary>
    /// Tries to get the party by specified host
    /// </summary>
    bool TryGetPartyByHost(ICommonSession host, [NotNullWhen(true)] out Party? party);

    /// <summary>
    /// Tries to get the party by specified member
    /// </summary>
    bool TryGetPartyByMember(ICommonSession member, [NotNullWhen(true)] out Party? party);

    /// <summary>
    /// Gets the party by specified <paramref name="id"/>
    /// </summary>
    Party? GetPartyById(uint id);

    /// <summary>
    /// Gets the party by specified host
    /// </summary>
    Party? GetPartyByHost(ICommonSession session);

    /// <summary>
    /// Gets the party by specified memer
    /// </summary>
    Party? GetPartyByMember(ICommonSession session);

    IEnumerable<CompletionOption> GetPartiesCompletionOptions();

    #region Settings
    /// <summary>
    /// Sets new settings in the <paramref name="party"/>
    /// </summary>
    /// <param name="updates">Should party update events be triggered or not</param>
    void SetPartySettings(Party party, PartySettings settings, bool updates = true);
    #endregion
}

public record struct PartyStatusChangedActionArgs(Party PartyId, PartyStatus OldStatus, PartyStatus NewStatus);
