using Content.Server.EUI;
using Content.Server.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly EuiManager _euiManager = default!;

    public event Action<PartyData>? OnPartyDataUpdated;
    public event Action<PartyData>? OnPartyDisbanded;

    public List<PartyData> Parties => _parties;
    private List<PartyData> _parties = new();

    public Dictionary<ICommonSession, PartyMenuEui> OpenedMenu => _openedMenu;
    private Dictionary<ICommonSession, PartyMenuEui> _openedMenu = new();

    /// <inheritdoc/>
    public bool TryCreateParty(NetUserId leader, [NotNullWhen(false)] out string? reason)
    {
        reason = null;

        try
        {
            CreateParty(leader);
            return true;
        }
        catch (Exception e)
        {
            reason = e.Message;
            return false;
        }
    }

    /// <inheritdoc/>
    public PartyData CreateParty(NetUserId leader)
    {
        CheckAvaliableMember(leader);
        var party = new PartyData(leader);
        _parties.Add(party);
        OnPartyDataUpdated?.Invoke(party);

        return party;
    }

    /// <inheritdoc/>
    public void DisbandParty(PartyData party)
    {
        OnPartyDisbanded?.Invoke(party);
        _parties.Remove(party);
    }

    /// <inheritdoc/>
    public PartyData? GetPartyByLeader(NetUserId leader)
    {
        return _parties.Find(x => x.IsLeader(leader));
    }

    /// <inheritdoc/>
    public PartyData? GetPartyByMember(NetUserId member)
    {
        return _parties.Find(x => x.ContainMember(member));
    }

    /// <inheritdoc/>
    public bool TryAddPlayerToParty(NetUserId member, PartyData party, [NotNullWhen(false)] out string? reason)
    {
        reason = null;

        try
        {
            AddPlayerToParty(member, party);
            return true;
        }
        catch (Exception e)
        {
            reason = e.Message;
            return false;
        }
    }

    /// <inheritdoc/>
    public void AddPlayerToParty(NetUserId member, PartyData party)
    {
        CheckAvaliableMember(member);

        party.AddMember(member);
        OnPartyDataUpdated?.Invoke(party);
    }

    /// <inheritdoc/>
    public void RemovePlayerFromParty(NetUserId member, PartyData party)
    {
        if (party.Leader == member)
            return;

        party.RemoveMember(member);
        OnPartyDataUpdated?.Invoke(party);
    }

    private bool CheckAvaliableMember(NetUserId member, bool throwExeption = true)
    {
        if (GetPartyByMember(member) != null)
        {
            if (throwExeption)
                throw new ArgumentException($"{member.UserId} is already the member of the another party");

            return false;
        }

        return true;
    }

    #region PartyMenuUI
    public void OpenPartyMenu(ICommonSession session)
    {
        if (_openedMenu.ContainsKey(session))
            return;

        var ui = new PartyMenuEui();
        _openedMenu.Add(session, ui);
        _euiManager.OpenEui(ui, session);
    }

    public void ClosePartyMenu(ICommonSession session)
    {
        if (!_openedMenu.TryGetValue(session, out var ui))
            return;

        _openedMenu.Remove(session);
        _euiManager.CloseEui(ui);
    }
    #endregion
}
