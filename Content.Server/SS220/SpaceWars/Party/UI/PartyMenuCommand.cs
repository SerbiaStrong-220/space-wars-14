using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.SS220.SpaceWars.Party.UI;

[AnyCommand]
public sealed class PartyMenuCommand : LocalizedCommands
{
    public override string Command => "partymenu";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        if (player == null)
            return;

        var partyManager = IoCManager.Resolve<IPartyManager>();
        partyManager.OpenPartyMenu(player);
    }
}
