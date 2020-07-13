namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemPragmiumSensorPrivateState : ItemWithDurabilityPrivateState
    {
        [TempOnly]
        public byte ServerCurrentSignalStrength { get; set; }

        [TempOnly]
        public double ServerTimeToPing { get; set; }

        [TempOnly]
        public double ServerTimeToPong { get; set; }
    }
}