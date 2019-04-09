namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBarrelRemovePetroleum
        : BaseRecipeBarrelRemoveLiquid<ItemCanisterEmpty, ItemCanisterPetroleum>
    {
        public override string Name => "Fill canister with raw petroleum oil from barrel";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;
    }
}