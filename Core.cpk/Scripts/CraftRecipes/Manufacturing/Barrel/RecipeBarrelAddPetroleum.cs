namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBarrelAddPetroleum
        : BaseRecipeBarrelAddLiquid<ItemCanisterPetroleum, ItemCanisterEmpty>
    {
        public override string Name => "Fill barrel with raw petroleum oil";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;
    }
}