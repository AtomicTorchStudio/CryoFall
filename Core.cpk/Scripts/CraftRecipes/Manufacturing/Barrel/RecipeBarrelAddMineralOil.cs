namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class RecipeBarrelAddMineralOil
        : BaseRecipeBarrelAddLiquid<ItemCanisterMineralOil, ItemCanisterEmpty>
    {
        public override string Name => "Fill barrel with mineral oil";
    }
}