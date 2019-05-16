namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    public interface IProtoObjectExtractor : IProtoObjectManufacturer
    {
        float LiquidCapacity { get; }

        float LiquidProductionAmountPerSecond { get; }
    }
}