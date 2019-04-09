namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectManufacturer : IProtoObjectStructure
    {
        void ClientSelectRecipe(IStaticWorldObject worldObject, Recipe recipe);
    }
}