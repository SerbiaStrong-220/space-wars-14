// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action<(Party? Old, Party? New)>? LocalPartyChanged;
    event Action<Party>? LocalPartyUpdated;

    PartyUIController UIController { get; }

    Party? LocalParty { get; }

    PartyEventsHandler LocalPartyEvents { get; }

    bool IsLocalPartyHost { get; }

    PartyMember? LocalMember { get; }

    /// <summary>
    /// Sends a request to create a new party with the specified <paramref name="settings"/>
    /// </summary>
    void CreatePartyRequest(PartySettingsState? settings = null);

    /// <summary>
    /// Sends a request to disband the <see cref="LocalParty"/>
    /// </summary>
    void DisbandPartyRequest();

    /// <summary>
    /// Sends a request to leave the <see cref="LocalParty"/>
    /// </summary>
    void LeavePartyRequest();

    /// <summary>
    /// Sends a request to set the <paramref name="userId"/> as a new host of the <see cref="LocalParty"/>
    /// </summary>
    void SetPartyHostRequest(NetUserId userId);

    /// <summary>
    /// Sends a request to kick the <paramref name="userId"/> from the <see cref="LocalParty"/>
    /// </summary>
    void KickFromPartyRequest(NetUserId userId);

    /// <summary>
    /// Sends a request to set a new settings in the <see cref="LocalParty"/>
    /// </summary>
    void SetSettingsRequest(PartySettingsState settingsState);
}

public sealed class PartyEventsHandler()
{
    public Action<(PartyStatus Old, PartyStatus New)>? StatusChanged;

    public Action<(PartyMember Old, PartyMember New)>? HostChanged;
    public Action<PartyMember>? HostUpdated;

    public Action<PartyMember>? MemberAdded;
    public Action<PartyMember>? MemberUpdated;
    public Action<PartyMember>? MemberRemoved;

    public Action<PartyInvite>? InviteAdded;
    public Action<PartyInvite>? InviteUpdated;
    public Action<PartyInvite>? InviteRemoved;
}
