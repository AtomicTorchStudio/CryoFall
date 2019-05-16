namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    public interface IProtoObjectWell : IProtoObjectManufacturer
    {
        float WaterCapacity { get; }

        float WaterProductionAmountPerSecond { get; }
    }
}