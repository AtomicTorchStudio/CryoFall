namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    public interface IProtoObjectWell : IProtoObjectManufacturer
    {
        double WaterCapacity { get; }

        double WaterProductionAmountPerSecond { get; }
    }
}