// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;

namespace Content.Client.SS220.SpaceWars.Party;

[Access(typeof(PartyManager), typeof(Party), Other = AccessPermissions.ReadExecute)]
public sealed class PartyMember() : IEquatable<PartyMember>
{
    public readonly NetUserId UserId;

    public string Username = string.Empty;
    public PartyMemberRole Role = PartyMemberRole.Invalid;
    public bool Connected;

    public PartyMember(PartyMemberState state) : this()
    {
        UserId = state.UserId;
        Role = state.Role;
        Username = state.Username;
        Connected = state.Connected;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PartyMember other)
            return false;

        return Equals(other);
    }

    public bool Equals(PartyMember? other)
    {
        if (other is null)
            return false;

        return UserId == other.UserId;
    }

    public static bool Equals(PartyMember? member1, PartyMember? member2)
    {
        if (member1 is null) return false;

        return member1.Equals(member2);
    }

    public static bool operator ==(PartyMember? left, PartyMember? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PartyMember? left, PartyMember? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return UserId.GetHashCode();
    }

    [Access(typeof(PartyManager), typeof(Party))]
    public void HandleState(PartyMemberState state)
    {
        if (UserId != state.UserId)
            return;

        Username = state.Username;
        Role = state.Role;
        Connected = state.Connected;
    }
}
