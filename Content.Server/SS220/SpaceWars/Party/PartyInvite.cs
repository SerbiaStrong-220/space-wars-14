// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party;

[Access([typeof(PartyManager), typeof(Party)], Other = AccessPermissions.ReadExecute)]
public sealed class PartyInvite(
    uint id,
    Party party,
    ICommonSession target,
    PartyInviteStatus status = PartyInviteStatus.Invalid) : IEquatable<PartyInvite>
{
    public readonly uint Id = id;
    public readonly Party Party = party;
    public readonly ICommonSession Target = target;

    public PartyInviteStatus Status = status;

    public PartyInviteState GetState()
    {
        return new PartyInviteState(Id, Status, Party.Id, Target.UserId, Party.Host.Name, Target.Name);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PartyInvite invite)
            return false;

        return Equals(invite);
    }

    public bool Equals(PartyInvite? other)
    {
        if (other is null)
            return false;

        return Id.Equals(other.Id);
    }

    public static bool Equals(PartyInvite? left, PartyInvite? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator ==(PartyInvite? left, PartyInvite? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PartyInvite? left, PartyInvite? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
