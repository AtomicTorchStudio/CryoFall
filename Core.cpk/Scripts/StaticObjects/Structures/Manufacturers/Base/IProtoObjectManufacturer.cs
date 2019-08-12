namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectManufacturer : IProtoObjectStructure
    {
        byte ContainerFuelSlotsCount { get; }

        byte ContainerInputSlotsCount { get; }

        byte ContainerOutputSlotsCount { get; }

        bool IsAutoSelectRecipe { get; }

        bool IsFuelProduceByproducts { get; }

        double ManufacturingSpeedMultiplier { get; }

        void ClientSelectRecipe(IStaticWorldObject worldObject, Recipe recipe);
    }
}