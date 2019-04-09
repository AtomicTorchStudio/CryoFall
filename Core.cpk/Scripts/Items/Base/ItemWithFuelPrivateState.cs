namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemWithFuelPrivateState : ItemWithDurabilityPrivateState
    {
        [SyncToClient(isSendChanges: false)]
        public double FuelAmount { get; set; }
    }
}