
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

[Serializable, NetSerializable]
public sealed class CreatePartyRequestMessage() : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed class CreatePartyResponceMessage(bool isCreated, string? reason = null) : EntityEventArgs
{
    public readonly bool IsCreated = isCreated;
    public readonly string? Reason = reason;
}

[Serializable, NetSerializable]
public sealed class PartyDataInfoRequestMessage() : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed class UpdatePartyDataInfoMessage(PartyData? partyData) : EntityEventArgs
{
    public readonly PartyData? PartyData = partyData;
}
