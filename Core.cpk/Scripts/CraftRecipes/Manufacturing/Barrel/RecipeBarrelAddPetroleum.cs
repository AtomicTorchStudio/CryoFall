namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class RecipeBarrelAddPetroleum
        : BaseRecipeBarrelAddLiquid<ItemCanisterPetroleum, ItemCanisterEmpty>
    {
        public override string Name => "Fill barrel with raw petroleum oil";
    }
}