namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectLightWithElectricityPrivateState
        : ObjectLightPrivateState, IObjectElectricityStructurePrivateState
    {
        [SyncToClient]
        public ElectricityThresholdsPreset ElectricityThresholds { get; set; }

        [SyncToClient]
        [TempOnly]
        public byte PowerGridChargePercent { get; set; }
    }
}