namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public interface IObjectElectricityStructurePrivateState : IPrivateState
    {
        ElectricityThresholdsPreset ElectricityThresholds { get; set; }

        byte PowerGridChargePercent { get; set; }
    }
}