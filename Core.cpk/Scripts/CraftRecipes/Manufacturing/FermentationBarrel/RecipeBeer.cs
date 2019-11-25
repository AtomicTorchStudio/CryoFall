namespace AtomicTorch.CBND.CoreMod.CraftRecipes.FermentationBarrel
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBeer : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectFermentationBarrel>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemWheatGrains>(count: 50);
            inputItems.Add<ItemSugar>(count: 10);

            outputItems.Add<ItemBeer>(count: 5);
        }
    }
}