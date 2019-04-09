namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBarrelAddMineralOil
        : BaseRecipeBarrelAddLiquid<ItemCanisterMineralOil, ItemCanisterEmpty>
    {
        public override string Name => "Fill barrel with mineral oil";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;
    }
}