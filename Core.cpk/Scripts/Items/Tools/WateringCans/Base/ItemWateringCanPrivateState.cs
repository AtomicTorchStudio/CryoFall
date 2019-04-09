namespace AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemWateringCanPrivateState : ItemWithDurabilityPrivateState
    {
        [SyncToClient(isSendChanges: false)]
        public byte WaterAmount { get; set; }
    }
}