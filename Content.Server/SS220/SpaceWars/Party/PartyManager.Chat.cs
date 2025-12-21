// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    private static readonly Dictionary<PartyChatMessageType, Color> MessageColors = new()
    {
        {PartyChatMessageType.Info, Color.Yellow},
        {PartyChatMessageType.Alert, Color.Red},
    };

    public void ChatMessageToParty(string message, Party party, PartyChatMessageType messageType = PartyChatMessageType.Common)
    {
        message = SanitizePartyChatMessage(message, messageType);
        foreach (var member in party.Members)
            SendPartyChatMessage(message, member.Session);
    }

    public void ChatMessageToUser(string message, ICommonSession session, PartyChatMessageType messageType = PartyChatMessageType.Common)
    {
        message = SanitizePartyChatMessage(message, messageType);
        SendPartyChatMessage(message, session);
    }

    private void SendPartyChatMessage(string message, ICommonSession session)
    {
        var msg = new MsgPartyChatMessage(message);
        SendNetMessage(msg, session);
    }

    private static string SanitizePartyChatMessage(string message, PartyChatMessageType messageType)
    {
        var msg = new FormattedMessage();

        if (MessageColors.TryGetValue(messageType, out var color))
            msg.PushColor(color);

        msg.AddText(message);
        msg.Pop();

        return msg.ToMarkup();
    }
}
