namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemWithFreshnessPrivateState : ItemPrivateState, IItemWithFreshnessPrivateState
    {
        /// <summary>
        /// Current value of freshness. Should never exceed the according item max freshness.
        /// </summary>
        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: 1)]
        public uint FreshnessCurrent { get; set; }
    }
}