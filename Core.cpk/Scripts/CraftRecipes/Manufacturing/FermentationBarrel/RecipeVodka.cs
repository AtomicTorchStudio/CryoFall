namespace AtomicTorch.CBND.CoreMod.CraftRecipes.FermentationBarrel
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeVodka : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectFermentationBarrel>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemCorn>(count: 15);
            inputItems.Add<ItemSugar>(count: 15);

            outputItems.Add<ItemVodka>(count: 5);
        }
    }
}