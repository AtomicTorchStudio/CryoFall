namespace AtomicTorch.CBND.CoreMod.CraftRecipes.FermentationBarrel
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeWine : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectFermentationBarrel>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemBerriesRed>(count: 10);
            inputItems.Add<ItemSugar>(count: 20);

            outputItems.Add<ItemWine>(count: 5);
        }
    }
}