namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBarrelRemoveWater
        : BaseRecipeBarrelRemoveLiquid<ItemBottleEmpty, ItemBottleWater>
    {
        public override string Name => "Fill bottle with water from barrel";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;
    }
}