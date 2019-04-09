namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemWithDurabilityPrivateState : BasePrivateState, IItemWithDurabilityPrivateState
    {
        /// <summary>
        /// Current value of durability. Should never exceed the according item max durability.
        /// </summary>
        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: 1)]
        public ushort DurabilityCurrent { get; set; }
    }
}