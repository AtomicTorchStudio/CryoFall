namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemFoodPrivateState : BasePrivateState, IFoodPrivateState
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