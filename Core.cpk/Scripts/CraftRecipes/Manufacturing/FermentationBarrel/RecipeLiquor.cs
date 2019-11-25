namespace AtomicTorch.CBND.CoreMod.CraftRecipes.FermentationBarrel
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeLiquor : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectFermentationBarrel>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemVodka>(count: 5);
            inputItems.Add<ItemBerriesRed>(count: 5);
            inputItems.Add<ItemHerbGreen>(count: 5);
            inputItems.Add<ItemSugar>(count: 2);

            outputItems.Add<ItemLiquor>(count: 5);
        }
    }
}