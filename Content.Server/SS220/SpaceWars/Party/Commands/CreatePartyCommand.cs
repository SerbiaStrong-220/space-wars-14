// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.SS220.SpaceWars.Party.Commands;

[AdminCommand(AdminFlags.Moderator)]
public sealed class CreatePartyCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _party = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override string Command => SharedPartyManager.CommandsPrefix + "create";
    public override string Description => Loc.GetString("cmd-create-party-desc");
    public override string Help => Loc.GetString("cmd-create-party-help");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1 || args.Length > 2)
        {
            shell.WriteError(Loc.GetString("cmd-create-party-invalid-arguments-count", ("help", Help)));
            return;
        }

        var hostName = args[0];
        if (!_player.TryGetSessionByUsername(hostName, out var host))
        {
            shell.WriteError(Loc.GetString("cmd-create-party-invalid-username", ("username", hostName)));
            return;
        }

        var force = false;
        if (args.Length >= 2 && !bool.TryParse(args[1], out force))
        {
            shell.WriteError(Loc.GetString("cmd-create-party-invalid-argument-2", ("arg", args[1])));
            return;
        }

        if (!force && _party.IsAnyPartyMember(host))
        {
            shell.WriteError(Loc.GetString("cmd-create-party-user-is-another-party-member", ("user", host.Name)));
            return;
        }

        if (!_party.CreateParty(host, out var party, force: force))
        {
            shell.WriteError(Loc.GetString("cmd-create-party-fail"));
            return;
        }

        shell.WriteLine(Loc.GetString("cmd-create-party-success", ("id", party.Id)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("cmd-create-party-hint-1")),
            2 => CompletionResult.FromHintOptions(CompletionHelper.Booleans, Loc.GetString("cmd-create-party-hint-2")),
            _ => CompletionResult.Empty
        };
    }
}
