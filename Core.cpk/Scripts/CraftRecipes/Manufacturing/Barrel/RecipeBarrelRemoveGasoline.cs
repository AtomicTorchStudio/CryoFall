namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBarrelRemoveGasoline
        : BaseRecipeBarrelRemoveLiquid<ItemCanisterEmpty, ItemCanisterGasoline>
    {
        public override string Name => "Fill canister with gasoline fuel from barrel";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;
    }
}