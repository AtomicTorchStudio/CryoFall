namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class PlayerTaskState : BaseNetObject
    {
        [SyncToClient(DeliveryMode.ReliableSequenced, maxUpdatesPerSecond: 1)]
        public bool IsCompleted { get; set; }
    }
}