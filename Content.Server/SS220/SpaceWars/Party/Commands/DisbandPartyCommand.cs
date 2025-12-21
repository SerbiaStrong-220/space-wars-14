// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Console;

namespace Content.Server.SS220.SpaceWars.Party.Commands;

[AdminCommand(AdminFlags.Moderator)]
public sealed class DisbandPartyCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _party = default!;

    public override string Command => SharedPartyManager.CommandsPrefix + "disband";
    public override string Description => Loc.GetString("cmd-disband-party-desc");
    public override string Help => Loc.GetString("cmd-disband-party-help");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("cmd-disband-party-invalid-arguments-count", ("help", Help)));
            return;
        }

        if (!uint.TryParse(args[0], out var partyId))
        {
            shell.WriteError(Loc.GetString("cmd-disband-party-invalid-argument-1", ("arg", args[0])));
            return;
        }

        if (!_party.TryGetPartyById(partyId, out var party))
        {
            shell.WriteError(Loc.GetString("cmd-disband-party-invalid-party-id", ("id", partyId)));
            return;
        }

        if (!_party.DisbandParty(party))
        {
            shell.WriteError(Loc.GetString("cmd-disband-party-fail", ("id", party.Id)));
            return;
        }

        shell.WriteLine(Loc.GetString("cmd-disband-party-success", ("id", party.Id)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(_party.GetPartiesCompletionOptions(), Loc.GetString("cmd-disband-party-hint-1")),
            _ => CompletionResult.Empty,
        };
    }
}
