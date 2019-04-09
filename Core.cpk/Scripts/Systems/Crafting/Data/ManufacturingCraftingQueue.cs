namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using JetBrains.Annotations;

    public class ManufacturingCraftingQueue : CraftingQueue
    {
        public ManufacturingCraftingQueue(
            [NotNull] IItemsContainer containerInput,
            [NotNull] IItemsContainer containerOutput) : base(containerInput, containerOutput)
        {
        }

        public IStaticWorldObject WorldObject => (IStaticWorldObject)this.GameObject;
    }
}