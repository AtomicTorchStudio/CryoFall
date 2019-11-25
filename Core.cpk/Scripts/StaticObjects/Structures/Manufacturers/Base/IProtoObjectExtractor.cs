namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    public interface IProtoObjectExtractor : IProtoObjectManufacturer
    {
        double LiquidCapacity { get; }

        double LiquidProductionAmountPerSecond { get; }
    }
}