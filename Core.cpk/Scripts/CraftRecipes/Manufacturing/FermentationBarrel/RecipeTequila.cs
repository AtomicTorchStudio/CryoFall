namespace AtomicTorch.CBND.CoreMod.CraftRecipes.FermentationBarrel
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeTequila : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectFermentationBarrel>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemCactusFlesh>(count: 30);
            inputItems.Add<ItemSugar>(count: 10);

            outputItems.Add<ItemTequila>(count: 5);
        }
    }
}