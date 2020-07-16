namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemPragmiumSensorPrivateState : ItemWithDurabilityPrivateState
    {
        [TempOnly]
        public double ServerTimeToPing { get; set; }
    }
}