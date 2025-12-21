// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Server.SS220.SpaceWars.Party;

public struct PartySettings()
{
    public int MembersLimit;

    public PartySettings(PartySettingsState state) : this()
    {
        MembersLimit = state.MembersLimit;
    }

    public readonly PartySettingsState GetState()
    {
        return new PartySettingsState(MembersLimit);
    }

    public static implicit operator PartySettings(PartySettingsState state)
    {
        return new PartySettings(state);
    }
}
