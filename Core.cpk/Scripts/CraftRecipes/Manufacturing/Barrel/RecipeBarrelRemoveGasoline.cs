namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class RecipeBarrelRemoveGasoline
        : BaseRecipeBarrelRemoveLiquid<ItemCanisterEmpty, ItemCanisterGasoline>
    {
        public override string Name => "Fill canister with gasoline fuel from barrel";
    }
}