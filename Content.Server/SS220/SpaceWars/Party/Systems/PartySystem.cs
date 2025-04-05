using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem : SharedPartySystem
{
    [Dependency] private readonly IPartyManager _partyManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        _partyManager.OnPartyDataUpdated += UpdateInfoForMembers;
        _partyManager.OnPartyDisbanded += OnPartyDisbanded;

        SubscribeNetworkEvent<CreatePartyRequestMessage>(OnCreatePartyRequest);
        SubscribeNetworkEvent<PartyDataInfoRequestMessage>(OnPartyDataInfoRequest);
    }

    private void OnCreatePartyRequest(CreatePartyRequestMessage message, EntitySessionEventArgs args)
    {
        var created = _partyManager.TryCreateParty(args.SenderSession.UserId, out var reason);
        var responce = new CreatePartyResponceMessage(created, reason);
        RaiseNetworkEvent(responce, args.SenderSession.Channel);
    }

    private void OnPartyDataInfoRequest(PartyDataInfoRequestMessage message, EntitySessionEventArgs args)
    {
        var party = _partyManager.GetPartyByMember(args.SenderSession.UserId);
        UpdatePartyData(args.SenderSession, party);
    }

    public void UpdatePartyData(NetUserId user, PartyData? party)
    {
        var session = _playerManager.GetSessionById(user);
        UpdatePartyData(session, party);
    }

    public void UpdatePartyData(ICommonSession session, PartyData? party)
    {
        var ev = new UpdatePartyDataInfoMessage(party);
        RaiseNetworkEvent(ev, session.Channel);
    }

    private void UpdateInfoForMembers(PartyData party)
    {
        foreach (var member in party.Members)
        {
            UpdatePartyData(member, party);
        }
    }

    private void OnPartyDisbanded(PartyData party)
    {
        foreach (var member in party.Members)
        {
            UpdatePartyData(member, null);
        }
    }
}
