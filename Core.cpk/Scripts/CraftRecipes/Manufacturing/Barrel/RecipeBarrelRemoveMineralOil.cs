namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class RecipeBarrelRemoveMineralOil
        : BaseRecipeBarrelRemoveLiquid<ItemCanisterEmpty, ItemCanisterMineralOil>
    {
        public override string Name => "Fill canister with mineral oil from barrel";
    }
}