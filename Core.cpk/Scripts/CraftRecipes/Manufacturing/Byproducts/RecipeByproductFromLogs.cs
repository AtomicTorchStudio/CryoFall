namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class RecipeByproductFromLogs : Recipe.RecipeForManufacturingByproduct
    {
        public override bool IsAutoUnlocked => true;

        public override string Name => "Logs burning byproduct";

        protected override void SetupRecipe(
            out TimeSpan craftDuration,
            out IProtoItem protoItemFuel,
            OutputItems outputItems)
        {
            var itemLogs = GetProtoEntity<ItemLogs>();

            // same as burning time of the item
            craftDuration = TimeSpan.FromSeconds(itemLogs.FuelAmount);

            protoItemFuel = itemLogs;

            outputItems.Add<ItemCharcoal>(probability: 0.75);
            outputItems.Add<ItemAsh>(probability: 0.75);
        }
    }
}