namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBarrelAddGasoline
        : BaseRecipeBarrelAddLiquid<ItemCanisterGasoline, ItemCanisterEmpty>
    {
        public override string Name => "Fill barrel with gasoline fuel";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;
    }
}