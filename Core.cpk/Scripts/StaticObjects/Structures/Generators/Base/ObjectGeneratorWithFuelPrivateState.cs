namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectGeneratorWithFuelPrivateState
        : ObjectManufacturerPrivateState, IObjectElectricityStructurePrivateState
    {
        [SyncToClient]
        public ElectricityThresholdsPreset ElectricityThresholds { get; set; }

        [TempOnly]
        public bool IsLiquidStatesChanged { get; set; }

        [SyncToClient]
        public LiquidContainerState LiquidState { get; set; }

        [SyncToClient]
        [TempOnly]
        public byte PowerGridChargePercent { get; set; }
    }
}