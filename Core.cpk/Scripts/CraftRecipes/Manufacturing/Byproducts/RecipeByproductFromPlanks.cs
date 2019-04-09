namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class RecipeByproductFromPlanks : Recipe.RecipeForManufacturingByproduct
    {
        public override bool IsAutoUnlocked => true;

        public override string Name => "Planks burning byproduct";

        protected override void SetupRecipe(
            out TimeSpan craftDuration,
            out IProtoItem protoItemFuel,
            OutputItems outputItems)
        {
            var itemPlanks = GetProtoEntity<ItemPlanks>();

            // same as burning time of the item
            craftDuration = TimeSpan.FromSeconds(itemPlanks.FuelAmount);

            protoItemFuel = itemPlanks;

            outputItems.Add<ItemCharcoal>(probability: 0.25);
            outputItems.Add<ItemAsh>(probability: 0.25);
        }
    }
}