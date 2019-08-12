namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;

    public interface IProtoObjectElectricityConsumer : IProtoObjectStructure
    {
        double ElectricityConsumptionPerSecondWhenActive { get; }
    }
}