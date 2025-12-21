// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using Robust.Shared.Enums;

namespace Content.Server.SS220.SpaceWars.Party;

[Access([typeof(PartyManager), typeof(Party)], Other = AccessPermissions.ReadExecute)]
public sealed class PartyMember(Party party, ICommonSession session, PartyMemberRole role = PartyMemberRole.Invalid) : IEquatable<PartyMember>
{
    public readonly Party Party = party;
    public readonly ICommonSession Session = session;
    public string Name => Session.Name;

    public PartyMemberRole Role = role;

    public PartyMemberState GetState()
    {
        var connected = Session.Status is SessionStatus.Connected or SessionStatus.InGame;
        return new PartyMemberState(Party.Id, Session.UserId, Session.Name, Role, connected);
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

        return Session == other.Session;
    }

    public static bool Equals(PartyMember? member1, PartyMember? member2)
    {
        if (ReferenceEquals(member1, member2))
            return true;

        if (member1 is null)
            return false;

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
        return Session.GetHashCode();
    }
}
