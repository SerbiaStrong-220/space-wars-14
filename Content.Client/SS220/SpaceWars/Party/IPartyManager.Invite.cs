// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<PartyInvite>? ReceivedInviteAdded;
    event Action<PartyInvite>? ReceivedInviteRemoved;
    event Action<PartyInvite>? ReceivedInviteUpdated;

    IReadOnlyList<PartyInvite> ReceivedInvites { get; }

    void InviteInitialize();

    /// <summary>
    /// Is the received invite with specified <paramref name="id"/> exist
    /// </summary>
    bool ReceivedInviteExist(uint id);

    /// <summary>
    /// Tries to get a received invite by specified <paramref name="id"/>
    /// </summary>
    bool TryGetReceivedInvite(uint id, [NotNullWhen(true)] out PartyInvite? invite);

    /// <summary>
    /// Gets a received invite by specified <paramref name="id"/>
    /// </summary>
    PartyInvite? GetReceivedInvite(uint id);

    /// <summary>
    /// Sends the request to invite a user with specified <paramref name="username"/> in the <see cref="LocalParty"/>
    /// </summary>
    void InviteUserRequest(string username);

    /// <summary>
    /// Sends the request to invite a user with specified <paramref name="username"/> in the <see cref="LocalParty"/>
    /// </summary>
    Task<MsgInviteUserResponce> InviteUserRequestAsync(string username);

    /// <summary>
    /// Sends the request to accept the received invite with specified <paramref name="inviteId"/>
    /// </summary>
    void AcceptReceivedInviteRequest(uint inviteId);

    /// <summary>
    /// Sends the request to deny the received invite with specified <paramref name="inviteId"/>
    /// </summary>
    void DenyReceivedInviteRequest(uint inviteId);

    /// <summary>
    /// Sends the request to delete the local party invite with specified <paramref name="inviteId"/>
    /// </summary>
    void DeleteLocalPartyInviteRequest(uint inviteId);

    void SetReceiveInvitesStatus(bool receiveInvites);
}
