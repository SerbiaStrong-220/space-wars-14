
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Threading.Tasks;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract partial class SharedPartyManager : ISharedPartyManager
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IDynamicTypeFactory _dynamicTypeFactory = default!;

    public const string CommandsPrefix = "party:";

    protected ISawmill Sawmill = default!;

    private readonly Dictionary<Type, INetSubscription> _netSubs = [];
    private event Action<PartyResponceMessage>? OnResponceMsg;

    // not synced
    private uint _nextMessageId = 0;

    public virtual void Initialize()
    {
        IoCManager.InjectDependencies(this);

        Sawmill = Logger.GetSawmill("PartyManager");

        _net.RegisterNetMessage<PartyNetMessage>(OnReceiveNetMessage);
    }

    private void OnReceiveNetMessage(PartyNetMessage msg)
    {
        if (msg.Message is not { } message)
            return;

        TryInvokeSubscription(message);

        switch (message)
        {
            case PartyResponceMessage responce:
                OnResponceMsg?.Invoke(responce);
                break;
        }

        bool TryInvokeSubscription(PartyMessage message)
        {
            var type = message.GetType();
            if (!_netSubs.TryGetValue(type, out var subscribe))
                return false;

            subscribe.Invoke(message);
            return true;
        }
    }

    protected void SubscribeNetMessage<T>(Action<T> callback) where T : PartyMessage
    {
        var type = typeof(T);
        if (_netSubs.ContainsKey(type))
            throw new Exception($"Already exist a subscription for an element with type: {type}");

        _netSubs.Add(type, new NetSubscription<T>(callback));
    }

    protected void SubscribeNetMessage<T>(Action<T, ICommonSession> callback) where T : PartyMessage
    {
        var type = typeof(T);
        if (_netSubs.ContainsKey(type))
            throw new Exception($"Already exist a subscription for an element with type: {type}");

        _netSubs.Add(type, new NetSubscription<T, ICommonSession>(callback));
    }

    protected async Task<T> WaitResponce<T>(TimeSpan? timeout = null, uint? id = null) where T : PartyResponceMessage, new()
    {
        var tcs = new TaskCompletionSource<T>();

        void OnResponce(PartyResponceMessage responce)
        {
            if (responce is not T result)
                return;

            if (id != result.Id)
                return;

            tcs.SetResult(result);
        }

        OnResponceMsg += OnResponce;

        timeout ??= TimeSpan.FromSeconds(4);
        var delayTask = Task.Delay(timeout.Value);

        var completedTask = await Task.WhenAny(tcs.Task, delayTask);

        T result;
        if (completedTask == tcs.Task)
            result = await tcs.Task;
        else
        {
            result = _dynamicTypeFactory.CreateInstance<T>();
            result.Timeout = true;
        }

        OnResponceMsg -= OnResponce;
        return result;
    }

    protected uint GenerateMessageId()
    {
        return _nextMessageId++;
    }

    private interface INetSubscription
    {
        void Invoke(PartyMessage message);
    }

    private sealed class NetSubscription<T>(Action<T> callback) : INetSubscription where T : PartyMessage
    {
        public Action<T> Callback = callback;

        public void Invoke(PartyMessage message)
        {
            Callback.Invoke((T)message);
        }
    }

    private sealed class NetSubscription<TMsg, TSession>(Action<TMsg, TSession> callback) : INetSubscription
        where TMsg : PartyMessage
        where TSession : ICommonSession
    {
        public Action<TMsg, TSession> Callback = callback;

        public void Invoke(PartyMessage message)
        {
            if (message.Sender is null)
            {
                DebugTools.Assert($"Failed to invoke a callback for \"{message.GetType()}\" by reason: " +
                    $"The field \"{nameof(PartyMessage.Sender)}\" of \"{message.GetType()}\" must not be null.");
                return;
            }

            var playerMng = IoCManager.Resolve<ISharedPlayerManager>();
            if (!playerMng.TryGetSessionById(message.Sender, out var session))
            {
                DebugTools.Assert($"Failed to invoke a callback for \"{message.GetType()}\" by reason: " +
                    $"Doesn't found a \"{nameof(ICommonSession)}\" for \"{nameof(NetUserId)}\" with id: \"{message.Sender.Value.UserId}\"");
                return;
            }

            Callback.Invoke((TMsg)message, (TSession)session);
        }
    }

    public static string GetPartyInviteStatusName(PartyInviteStatus status)
    {
        return Loc.GetString($"party-invite-status-{status.ToString().ToLower()}");
    }

    public static string GetPartyMemberRoleName(PartyMemberRole role)
    {
        return Loc.GetString($"party-member-role-{role.ToString().ToLower()}");
    }
}
