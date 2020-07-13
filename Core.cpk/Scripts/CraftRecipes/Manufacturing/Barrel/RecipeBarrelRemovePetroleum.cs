namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class RecipeBarrelRemovePetroleum
        : BaseRecipeBarrelRemoveLiquid<ItemCanisterEmpty, ItemCanisterPetroleum>
    {
        public override string Name => "Fill canister with raw petroleum oil from barrel";
    }
}