namespace AtomicTorch.CBND.CoreMod.Systems.LiquidContainer
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class LiquidContainerState : BaseNetObject
    {
        [SyncToClient(DeliveryMode.UnreliableSequenced, maxUpdatesPerSecond: 2, networkDataType: typeof(float))]
        public double Amount { get; set; }
    }
}