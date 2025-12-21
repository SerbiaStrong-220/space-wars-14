// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Console;
using System.Text;

namespace Content.Server.SS220.SpaceWars.Party.Commands;

[AdminCommand(AdminFlags.Moderator)]
public sealed class ListPartiesCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _party = default!;

    public override string Command => SharedPartyManager.CommandsPrefix + "list";
    public override string Description => Loc.GetString("cmd-list-parties-desc");
    public override string Help => Loc.GetString("cmd-list-parties-help");

    private string IdTitle => Loc.GetString("cmd-list-parties-id-title");
    private string HostTitle => Loc.GetString("cmd-list-parties-host-title");
    private string MembersTitle => Loc.GetString("cmd-list-parties-members-title");
    private string StatusTitle => Loc.GetString("cmd-list-parties-status-title");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var parties = _party.Parties;
        if (parties.Count == 0)
        {
            shell.WriteError(Loc.GetString("cmd-list-parties-zero-paries"));
            return;
        }

        var format = GetStringFormat(parties);

        var builder = new StringBuilder();
        var title = string.Format(format, IdTitle, HostTitle, MembersTitle, StatusTitle);
        builder.AppendLine(title);

        var devider = new string('=', title.Length);
        builder.AppendLine(devider);

        foreach (var party in parties)
        {
            var partyLine = string.Format(format, party.Id, party.Host.Session.Name, party.Members.Count, party.Status.ToString());
            builder.AppendLine(partyLine);
        }

        shell.WriteLine(builder.ToString());
    }

    private string GetStringFormat(IEnumerable<Party> parties)
    {
        const int tab = 4;

        var idWidth = tab;
        var hostWidth = tab;
        var membersWidth = tab;
        var statusWidth = tab;

        CalculateExtraWidth(IdTitle, ref idWidth);
        CalculateExtraWidth(HostTitle, ref hostWidth);
        CalculateExtraWidth(MembersTitle, ref membersWidth);
        CalculateExtraWidth(StatusTitle, ref statusWidth);

        foreach (var party in parties)
        {
            CalculateExtraWidth(party.Id.ToString(), ref idWidth);
            CalculateExtraWidth(party.Host.Session.Name, ref hostWidth);
            CalculateExtraWidth(party.Members.Count.ToString(), ref membersWidth);
            CalculateExtraWidth(party.Status.ToString(), ref statusWidth);
        }

        return $"{{0,-{idWidth}}}{{1,-{hostWidth}}}{{2,-{membersWidth}}}{{3,-{statusWidth}}}";

        static void CalculateExtraWidth(string param, ref int paramWidth)
        {
            var diff = param.Length - paramWidth;

            switch (diff)
            {
                case int i when i == 0:
                    paramWidth += tab;
                    return;

                case int i when i > 0:
                    paramWidth += (int)Math.Ceiling((double)diff / tab) * tab;
                    return;

                default:
                    return;
            }
        }
    }
}
