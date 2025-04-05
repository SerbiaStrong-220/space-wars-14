using Content.Shared.Eui;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

public static class PartyMenuEuiMsg
{
    [Serializable, NetSerializable]
    public sealed class CreateParty() : EuiMessageBase
    {
    }

    [Serializable, NetSerializable]
    public sealed class DisbandParty() : EuiMessageBase
    {

    }

    [Serializable, NetSerializable]
    public sealed class LeaveParty() : EuiMessageBase
    {
    }
}
