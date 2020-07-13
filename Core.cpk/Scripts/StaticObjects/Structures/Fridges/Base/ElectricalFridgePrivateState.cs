namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ElectricalFridgePrivateState : ObjectCratePrivateState, IObjectElectricityStructurePrivateState
    {
        [SyncToClient]
        public ElectricityThresholdsPreset ElectricityThresholds { get; set; }

        [SyncToClient]
        [TempOnly]
        public byte PowerGridChargePercent { get; set; }
    }
}