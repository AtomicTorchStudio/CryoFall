namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemWithActiveFlagPublicState : BasePublicState
    {
        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            receivers: SyncToClientReceivers.ScopePlayers)]
        public bool IsActive { get; set; }
    }
}