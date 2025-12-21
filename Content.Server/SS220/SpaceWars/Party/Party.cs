// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

[Access(typeof(PartyManager))]
public sealed class Party : IEquatable<Party>, IDisposable
{
    [Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
    public readonly uint Id;

    [Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
    public PartyStatus Status = PartyStatus.Invalid;

    [Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
    public PartyMember Host { get; private set; }

    [Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
    public IReadOnlyCollection<PartyMember> Members => _members;
    private readonly HashSet<PartyMember> _members = [];

    [Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
    public IReadOnlyCollection<PartyInvite> Invites => _invites;
    private readonly HashSet<PartyInvite> _invites = [];

    [Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
    public PartySettings Settings;

    [Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
    public bool MembersLimitReached => Members.Count >= Settings.MembersLimit;

    private bool _disposed;

    public Party(uint id, ICommonSession host, PartySettings? settings = null)
    {
        Id = id;

        Host = new PartyMember(this, host, PartyMemberRole.Host);
        _members.Add(Host);

        Settings = settings ?? new();
    }

    public bool SetHost(ICommonSession session, bool ignoreLimit = false)
    {
        return SetHost(session, out _, ignoreLimit);
    }

    public bool SetHost(ICommonSession session, [NotNullWhen(true)] out PartyMember? host, bool ignoreLimit = false)
    {
        DebugTools.Assert(!_disposed);

        host = null;
        if (Host.Session == session)
            return false;

        var oldHost = Host;
        if (TryFindMember(session, out host))
        {
            host.Role = PartyMemberRole.Host;
        }
        else
        {
            if (!ignoreLimit && MembersLimitReached)
                return false;

            host = new PartyMember(this, session, PartyMemberRole.Host);
            _members.Add(host);
        }

        Host = host;
        oldHost.Role = PartyMemberRole.Member;
        return true;
    }

    public bool AddMember(ICommonSession session, PartyMemberRole role, bool ignoreLimit = false)
    {
        return AddMember(session, role, out _, ignoreLimit);
    }

    public bool AddMember(ICommonSession session, PartyMemberRole role, [NotNullWhen(true)] out PartyMember? member, bool ignoreLimit = false)
    {
        DebugTools.Assert(!_disposed);

        member = null;
        if (!CanAddMember(session, role, ignoreLimit))
            return false;

        member = new PartyMember(this, session, role);
        return _members.Add(member);
    }

    [Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
    public bool ContainsMember(ICommonSession session)
    {
        DebugTools.Assert(!_disposed);
        return TryFindMember(session, out _);
    }

    public bool TryFindMember(ICommonSession session, [NotNullWhen(true)] out PartyMember? member)
    {
        DebugTools.Assert(!_disposed);

        var sorted = _members.Where(x => x.Session == session);
        var count = sorted.Count();
        if (count <= 0)
        {
            member = null;
            return false;
        }

        DebugTools.Assert(sorted.Count() == 1, $"The user with the nickname {session.Name} is listed more than once in the party members. Party_id: \"{Id}\"");
        member = sorted.First();
        return true;
    }

    public bool RemoveMember(ICommonSession session)
    {
        DebugTools.Assert(!_disposed);

        if (!TryFindMember(session, out var member))
            return false;

        return RemoveMember(member);
    }

    public bool RemoveMember(PartyMember member)
    {
        DebugTools.Assert(!_disposed);
        DebugTools.Assert(!IsHost(member));

        return _members.Remove(member);
    }

    [Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
    public bool IsHost(PartyMember member)
    {
        DebugTools.Assert(!_disposed);
        return IsHost(member.Session);
    }

    [Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
    public bool IsHost(ICommonSession session)
    {
        DebugTools.Assert(!_disposed);
        return Host.Session == session;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _members.Clear();
        _disposed = true;
    }

    public bool AddInvite(PartyInvite invite)
    {
        DebugTools.Assert(!_disposed);
        return _invites.Add(invite);
    }

    public bool RemoveInvite(PartyInvite invite)
    {
        DebugTools.Assert(!_disposed);
        return _invites.Remove(invite);
    }

    public PartyState GetState()
    {
        DebugTools.Assert(!_disposed);
        return new PartyState(
            Id,
            Host.GetState(),
            [.. _members.Select(x => x.GetState())],
            [.. _invites.Select(x => x.GetState())],
            Settings.GetState(),
            Status);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Party other)
            return false;

        return Equals(other);
    }

    public bool Equals(Party? other)
    {
        if (other is null)
            return false;

        return Id == other.Id;
    }

    public static bool Equals(Party? party1, Party? party2)
    {
        if (ReferenceEquals(party1, party2))
            return true;

        if (party1 is null) return false;

        return party1.Equals(party2);
    }

    public static bool operator ==(Party? left, Party? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Party? left, Party? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    private bool CanAddMember(ICommonSession session, PartyMemberRole role, bool ignoreLimit = false)
    {
        /// Cannot add member with the <see cref="PartyMemberRole.Host"/> role.
        /// Should use <see cref="SetHost(ICommonSession, bool)"/> to set a new party host
        if (role is PartyMemberRole.Host)
            return false;

        if (TryFindMember(session, out _))
            return false;

        return ignoreLimit || !MembersLimitReached;
    }
}
