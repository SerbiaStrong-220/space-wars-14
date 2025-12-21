// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Console;
using System.Text;

namespace Content.Server.SS220.SpaceWars.Party.Commands;

[AdminCommand(AdminFlags.Moderator)]
public sealed class PartyInfoCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _party = default!;

    public override string Command => SharedPartyManager.CommandsPrefix + "info";
    public override string Description => Loc.GetString("cmd-party-info-desc");
    public override string Help => Loc.GetString("cmd-party-info-help");

    private string IdLineName => Loc.GetString("cmd-party-info-id-line-name");
    private string HostLineName => Loc.GetString("cmd-party-info-host-line-name");
    private string MembersLineName => Loc.GetString("cmd-party-info-members-line-name");
    private string StatusLineName => Loc.GetString("cmd-party-info-status-line-name");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("cmd-party-info-invalid-arguments-count", ("help", Help)));
            return;
        }

        if (!uint.TryParse(args[0], out var partyId))
        {
            shell.WriteError(Loc.GetString("cmd-party-info-invalid-argument-1", ("arg", args[0])));
            return;
        }

        if (!_party.TryGetPartyById(partyId, out var party))
        {
            shell.WriteError(Loc.GetString("cmd-party-info-invalid-party-id", ("id", partyId)));
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine($"{IdLineName}: {party.Id}");
        builder.AppendLine($"{HostLineName}: {party.Host.Session.Name}");
        builder.AppendLine($"{MembersLineName}:");
        foreach (var member in party.Members)
            builder.AppendLine($"- {member.Session.Name}");

        builder.AppendLine($"{StatusLineName}: {party.Status.ToString()}");

        shell.WriteLine(builder.ToString());
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(_party.GetPartiesCompletionOptions(), Loc.GetString("cmd-party-info-hint-1")),
            _ => CompletionResult.Empty
        };
    }
}
