// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;

namespace Content.Client.SS220.SpaceWars.Party;

[Access([typeof(PartyManager), typeof(Party)], Other = AccessPermissions.ReadExecute)]
public sealed class PartyInvite(PartyInviteState state) : IEquatable<PartyInvite>
{
    public readonly uint Id = state.Id;
    public PartyInviteStatus Status = state.Status;

    public uint PartyId = state.PartyId;
    public NetUserId Target = state.Target;
    public string SenderName = state.SenderName;
    public string TargetName = state.TargetName;

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

    [Access([typeof(PartyManager), typeof(Party)])]
    public void HandleState(PartyInviteState state)
    {
        if (Id != state.Id)
            return;

        Status = state.Status;
        PartyId = state.PartyId;
        Target = state.Target;
        SenderName = state.SenderName;
        TargetName = state.TargetName;
    }
}
