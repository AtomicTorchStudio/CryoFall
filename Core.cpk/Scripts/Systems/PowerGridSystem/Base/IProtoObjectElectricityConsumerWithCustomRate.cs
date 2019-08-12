namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectElectricityConsumerWithCustomRate : IProtoObjectElectricityConsumer
    {
        double SharedGetCurrentElectricityConsumptionRate(IStaticWorldObject worldObject);
    }
}