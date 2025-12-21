// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<PartyInviteStatusChangedActionArgs>? PartyInviteStatusChanged;

    IReadOnlyList<PartyInvite> Invites { get; }

    /// <summary>
    /// Accepts the <paramref name="invite"/>
    /// </summary>
    /// <param name="ignoreLimit">Should the party invites limit be ignored or not</param>
    /// <returns><see langword="true"/> if the <paramref name="invite"/> was accepted</returns>
    bool AcceptInvite(PartyInvite invite, bool ignoreLimit = false);

    /// <summary>
    /// Denies the <paramref name="invite"/>
    /// </summary>
    /// <returns><see langword="true"/> if the <paramref name="invite"/> was denied</returns>
    void DenyInvite(PartyInvite invite);

    /// <inheritdoc cref="CreateInvite(Party, ICommonSession, out PartyInviteCheckoutResult, out PartyInvite?)"/>
    bool CreateInvite(Party party, ICommonSession target, [NotNullWhen(true)] out PartyInvite? invite);

    /// <summary>
    /// Creates a new invite from the <paramref name="party"/> to the <paramref name="target"/>
    /// </summary>
    /// <returns><see langword="true"/> if a new invite was created</returns>
    bool CreateInvite(Party party, ICommonSession target, out PartyInviteCheckoutResult result, [NotNullWhen(true)] out PartyInvite? invite);

    /// <summary>
    /// Deletes an invite with specified <paramref name="inviteId"/>
    /// </summary>
    /// <returns><see langword="true"/> if an invite was deleted</returns>
    bool DeleteInvite(uint inviteId);

    /// <summary>
    /// Deletes an invite from the <paramref name="party"/> to the <paramref name="target"/>
    /// </summary>
    /// <returns><see langword="true"/> if an invite was deleted</returns>
    bool DeleteInvite(Party party, ICommonSession target);

    /// <summary>
    /// Deletes the <paramref name="invite"/>
    /// </summary>
    /// <returns><see langword="true"/> if the <paramref name="invite"/> was deleted</returns>
    bool DeleteInvite(PartyInvite invite);

    /// <summary>
    /// Is available to create an invite from the <paramref name="party"/> to the <paramref name="target"/>
    /// </summary>
    /// <returns><see langword="true"/> if available to create an invite</returns>
    bool InviteAvailable(Party party, ICommonSession target);

    /// <summary>
    /// Is available to create an invite from the <paramref name="party"/> to the <paramref name="target"/>
    /// </summary>
    /// <returns><see langword="true"/> if available to create an invite</returns>
    bool InviteAvailableCheckout(Party party, ICommonSession target, out PartyInviteCheckoutResult result, bool ingoreMembersLimit = false);

    /// <summary>
    /// Tries to get the <paramref name="invite"/> by specified <paramref name="inviteId"/>
    /// </summary>
    bool TryGetInvite(uint inviteId, [NotNullWhen(true)] out PartyInvite? invite);

    /// <summary>
    /// Tries to get the <paramref name="invite"/> by specified <paramref name="party"/> and <paramref name="target"/>
    /// </summary>
    bool TryGetInvite(Party party, ICommonSession target, [NotNullWhen(true)] out PartyInvite? invite);

    /// <summary>
    /// Gets an invite by specified <paramref name="inviteId"/>
    /// </summary>
    PartyInvite? GetInvite(uint inviteId);

    /// <summary>
    /// Gets an invite by specified specified <paramref name="party"/> and <paramref name="target"/>
    /// </summary>
    PartyInvite? GetInvite(Party party, ICommonSession target);

    /// <summary>
    /// Gets all invites sended from the <paramref name="party"/>
    /// </summary>
    IEnumerable<PartyInvite> GetInvitesByParty(Party party);

    /// <summary>
    /// Gets all invites sended to the <paramref name="target"/>
    /// </summary>
    IEnumerable<PartyInvite> GetInvitesByTarget(ICommonSession target);

    /// <summary>
    /// Sets a new status in the <paramref name="invite"/>
    /// </summary>
    /// <param name="updates">Should party update events be triggered or not</param>
    void SetInviteStatus(PartyInvite invite, PartyInviteStatus status, bool updates = true);

    /// <summary>
    /// Is the <paramref name="invite"/> exist
    /// </summary>
    bool InviteExist(PartyInvite? invite);
}

public enum PartyInviteCheckoutResult
{
    Available,
    PartyNotExist,
    AlreadyMember,
    InvitesLimitReached,
    MembersLimitReached,
    AlreadyInvited,
    DoesNotReseive
}

public record struct PartyInviteStatusChangedActionArgs(uint Id, PartyInviteStatus OldStatus, PartyInviteStatus NewStatus);
