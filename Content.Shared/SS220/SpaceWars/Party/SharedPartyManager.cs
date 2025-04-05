
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using System.Linq;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract partial class SharedPartyManager : ISharedPartyManager
{
    protected ISawmill _sawmill = default!;

    public virtual void Initialize()
    {
        IoCManager.InjectDependencies(this);

        _sawmill = Logger.GetSawmill("PartyManager");
    }
}

[Serializable, NetSerializable]
[Access(typeof(SharedPartyManager), Other = AccessPermissions.ReadExecute)]
public sealed class PartyData
{
    public NetUserId? Leader => GetLeader();

    public NetUserId[] Members;

    /// <summary>
    ///     The first session in the array will be considered the leader
    /// </summary>
    public PartyData(NetUserId[] players)
    {
        Members = players;
    }

    public PartyData(NetUserId leader) : this([leader]) { }

    public NetUserId? GetLeader()
    {
        if (Members.Length <= 0)
            return null;

        return Members[0];
    }

    public bool IsLeader(NetUserId session)
    {
        return Leader == session;
    }

    public bool ContainMember(NetUserId session)
    {
        var contains = false;
        for (var i = 0; i < Members.Length; i++)
        {
            if (Members[i] == session)
                contains = true;
        }

        return contains;
    }

    [Access(typeof(SharedPartyManager))]
    public void AddMember(NetUserId member)
    {
        var newArray = new NetUserId[Members.Length + 1];
        Members.CopyTo(newArray, 0);
        newArray[^1] = member;

        Members = newArray;
    }

    [Access(typeof(SharedPartyManager))]
    public void RemoveMember(NetUserId member)
    {
        if (!ContainMember(member))
            return;

        var newArray = new NetUserId[Members.Length - 1];
        var accumulate = 0;
        for (var i = 0; i < Members.Length; i++)
        {
            if (Members[i] == member)
                continue;

            newArray[accumulate] = Members[i];
            accumulate++;
        }

        Members = newArray;
    }
}
