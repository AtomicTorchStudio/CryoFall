namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class RecipeBarrelRemoveWater
        : BaseRecipeBarrelRemoveLiquid<ItemBottleEmpty, ItemBottleWater>
    {
        public override string Name => "Fill bottle with water from barrel";
    }
}