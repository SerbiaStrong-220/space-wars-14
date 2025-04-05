using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;

namespace Content.Client.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem : SharedPartySystem
{
    [Dependency] private readonly IPartyManager _partyManager = default!;

    public Action<bool, string?>? CreatedPartyResponce;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CreatePartyResponceMessage>(OnCreatePartyResponce);
        SubscribeNetworkEvent<UpdatePartyDataInfoMessage>(OnUpdatePartyDataInfo);
    }

    private void OnCreatePartyResponce(CreatePartyResponceMessage message)
    {
        CreatedPartyResponce?.Invoke(message.IsCreated, message.Reason);
    }

    private void OnUpdatePartyDataInfo(UpdatePartyDataInfoMessage message)
    {
        _partyManager.SetPartyData(message.PartyData);
    }

    public void SendCreatePartyRequest()
    {
        var request = new CreatePartyRequestMessage();
        RaiseNetworkEvent(request);
    }
}
