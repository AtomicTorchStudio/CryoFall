namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectGeneratorPrivateState : StructurePrivateState, IObjectElectricityStructurePrivateState
    {
        [SyncToClient]
        public ElectricityThresholdsPreset ElectricityThresholds { get; set; }

        [SyncToClient]
        [TempOnly]
        public byte PowerGridChargePercent { get; set; }
    }
}