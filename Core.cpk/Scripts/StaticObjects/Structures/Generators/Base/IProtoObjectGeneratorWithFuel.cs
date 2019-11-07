namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;

    public interface IProtoObjectGeneratorWithFuel : IProtoObjectElectricityProducer
    {
        byte ContainerFuelSlotsCount { get; }

        double LiquidCapacity { get; }

        LiquidType LiquidType { get; }
    }
}