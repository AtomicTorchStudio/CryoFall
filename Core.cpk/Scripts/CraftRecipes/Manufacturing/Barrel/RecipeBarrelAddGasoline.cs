namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class RecipeBarrelAddGasoline
        : BaseRecipeBarrelAddLiquid<ItemCanisterGasoline, ItemCanisterEmpty>
    {
        public override string Name => "Fill barrel with gasoline fuel";
    }
}