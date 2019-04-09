namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class RecipeByproductFromTwigs : Recipe.RecipeForManufacturingByproduct
    {
        public override bool IsAutoUnlocked => true;

        public override string Name => "Twigs burning byproduct";

        protected override void SetupRecipe(
            out TimeSpan craftDuration,
            out IProtoItem protoItemFuel,
            OutputItems outputItems)
        {
            var itemTwigs = GetProtoEntity<ItemTwigs>();

            // same as burning time of the item
            craftDuration = TimeSpan.FromSeconds(itemTwigs.FuelAmount);

            protoItemFuel = itemTwigs;

            outputItems.Add<ItemCharcoal>(probability: 0.33);
            outputItems.Add<ItemAsh>(probability: 0.33);
        }
    }
}