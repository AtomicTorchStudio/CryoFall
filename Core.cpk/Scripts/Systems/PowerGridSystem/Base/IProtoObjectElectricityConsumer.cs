namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectElectricityConsumer : IProtoObjectStructure
    {
        ElectricityThresholdsPreset DefaultConsumerElectricityThresholds { get; }

        double ElectricityConsumptionPerSecondWhenActive { get; }

        IObjectElectricityStructurePrivateState GetPrivateState(IStaticWorldObject worldObject);

        IObjectElectricityConsumerPublicState GetPublicState(IStaticWorldObject worldObject);
    }
}