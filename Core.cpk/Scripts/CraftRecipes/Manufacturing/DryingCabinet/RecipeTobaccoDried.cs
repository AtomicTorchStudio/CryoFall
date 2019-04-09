namespace AtomicTorch.CBND.CoreMod.CraftRecipes.DryingCabinet
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeTobaccoDried : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectDryingCabinet>();

            duration = CraftingDuration.Long;

            inputItems.Add<ItemTobaccoRaw>();

            outputItems.Add<ItemTobaccoDried>();
        }
    }
}