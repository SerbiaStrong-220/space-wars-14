// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Client.SS220.SpaceWars.Party;

[Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
public sealed class Party
{
    public readonly uint Id;

    public PartyMember Host;
    public PartyStatus Status;

    public IReadOnlyList<PartyMember> Members => [.. _members.Values];
    private readonly Dictionary<NetUserId, PartyMember> _members = [];

    public IReadOnlyList<PartyInvite> Invites => [.. _invites.Values];
    private readonly Dictionary<uint, PartyInvite> _invites = [];

    public PartySettings Settings;

    public PartyEventsHandler? EventsHandler;

    public Party(PartyState state)
    {
        Id = state.Id;

        DebugTools.Assert(state.Host.Role is PartyMemberRole.Host);
        Host = new PartyMember(state.Host);
        Status = state.Status;

        _members = state.Members.ToDictionary(x => x.UserId, x => new PartyMember(x));
        _invites = state.Invites.ToDictionary(x => x.Id, x => new PartyInvite(x));
        Settings = state.Settings;
    }

    public bool TryFindMember(NetUserId userId, [NotNullWhen(true)] out PartyMember? member)
    {
        if (_members.TryGetValue(userId, out var exist))
        {
            member = exist;
            return true;
        }
        else
        {
            member = null;
            return false;
        }
    }

    public PartyMember? FindMember(NetUserId userId)
    {
        TryFindMember(userId, out var member);
        return member;
    }

    public bool IsHost(NetUserId userId)
    {
        return Host.UserId == userId;
    }

    public bool TryGetInvite(uint id, [NotNullWhen(true)] out PartyInvite? invte)
    {
        return _invites.TryGetValue(id, out invte);
    }

    [Access(typeof(PartyManager))]
    public void HandleState(PartyState state)
    {
        if (Id != state.Id)
            return;

        if (Host.UserId != state.Host.UserId)
        {
            DebugTools.Assert(state.Host.Role is PartyMemberRole.Host);

            var oldHost = Host;
            Host = new PartyMember(state.Host);
            EventsHandler?.HostChanged?.Invoke((oldHost, Host));
        }
        else
        {
            Host.HandleState(state.Host);
            EventsHandler?.HostUpdated?.Invoke(Host);
        }

        if (Status != state.Status)
        {
            var oldStatus = Status;
            Status = state.Status;
            EventsHandler?.StatusChanged?.Invoke((oldStatus, Status));
        }

        UpdateMembers(state.Members);
        UpdateInvites(state.Invites);

        Settings = state.Settings;
    }

    private void UpdateMembers(List<PartyMemberState> memberStates)
    {
        var toRemove = _members.Values.ToList();
        foreach (var state in memberStates)
        {
            var userId = state.UserId;
            if (_members.TryGetValue(userId, out var exist))
            {
                exist.HandleState(state);
                EventsHandler?.MemberUpdated?.Invoke(exist);
                toRemove.Remove(exist);
            }
            else
            {
                var member = new PartyMember(state);
                _members.Add(userId, member);
                EventsHandler?.MemberAdded?.Invoke(member);
            }
        }

        foreach (var member in toRemove)
        {
            if (_members.Remove(member.UserId))
                EventsHandler?.MemberRemoved?.Invoke(member);
        }

    }

    private void UpdateInvites(List<PartyInviteState> inviteStates)
    {
        var toRemove = _invites.Values.ToList();
        foreach (var state in inviteStates)
        {
            if (_invites.TryGetValue(state.Id, out var exist))
            {
                exist.HandleState(state);
                EventsHandler?.InviteUpdated?.Invoke(exist);
                toRemove.Remove(exist);
            }
            else
            {
                var invite = new PartyInvite(state);
                _invites.Add(state.Id, invite);
                EventsHandler?.InviteAdded?.Invoke(invite);
            }
        }

        foreach (var invite in toRemove)
        {
            if (_invites.Remove(invite.Id))
                EventsHandler?.InviteRemoved?.Invoke(invite);
        }
    }
}
