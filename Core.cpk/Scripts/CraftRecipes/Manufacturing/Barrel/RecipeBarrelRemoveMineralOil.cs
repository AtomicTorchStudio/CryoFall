namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBarrelRemoveMineralOil
        : BaseRecipeBarrelRemoveLiquid<ItemCanisterEmpty, ItemCanisterMineralOil>
    {
        public override string Name => "Fill canister with mineral oil from barrel";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;
    }
}