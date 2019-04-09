namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBarrelAddWater
        : BaseRecipeBarrelAddLiquid<ItemBottleWater, ItemBottleEmpty>
    {
        public override string Name => "Fill barrel with water";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;
    }
}