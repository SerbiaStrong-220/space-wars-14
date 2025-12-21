// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using JetBrains.Annotations;
using Robust.Shared.Console;

namespace Content.Client.SS220.SpaceWars.Party.Commands;

[UsedImplicitly]
public sealed class PartyWindowCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _party = default!;

    public override string Command => SharedPartyManager.CommandsPrefix + "window";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _party.UIController.ToggleWindow();
    }
}
