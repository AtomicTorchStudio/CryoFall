namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class RecipeBarrelAddWater
        : BaseRecipeBarrelAddLiquid<ItemBottleWater, ItemBottleEmpty>
    {
        public override string Name => "Fill barrel with water";
    }
}