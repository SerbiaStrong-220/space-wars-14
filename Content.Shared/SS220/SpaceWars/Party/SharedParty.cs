// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

#region Party
[Serializable, NetSerializable]
public record struct PartyState(
    uint Id,
    PartyMemberState Host,
    List<PartyMemberState> Members,
    List<PartyInviteState> Invites,
    PartySettingsState Settings,
    PartyStatus Status);

[Serializable, NetSerializable]
public enum PartyStatus : byte
{
    Invalid,
    Running,
    Disbanding,
    Disbanded
}
#endregion

#region PartyMember
[Serializable, NetSerializable]
public record struct PartyMemberState(uint PartyId, NetUserId UserId, string Username, PartyMemberRole Role, bool Connected);

[Serializable, NetSerializable]
public enum PartyMemberRole : byte
{
    Invalid,

    /// <summary>
    /// Default role of the party member
    /// </summary>
    Member,

    /// <summary>
    /// Host of the party
    /// </summary>
    Host
}
#endregion

#region PartyInvite
[Serializable, NetSerializable]
public record struct PartyInviteState(
    uint Id,
    PartyInviteStatus Status,
    uint PartyId,
    NetUserId Target,
    string SenderName,
    string TargetName);

[Serializable, NetSerializable]
public enum PartyInviteStatus : byte
{
    Invalid,
    Created,
    Sended,

    Accepted,
    Denied,

    Deleted
}
#endregion

#region Settings
[Serializable, NetSerializable]
public record struct PartySettingsState(int MembersLimit);
#endregion
