namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeChiliBeans : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectStove>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemCannedBeans>(count: 1);
            inputItems.Add<ItemBeer>(count: 1);
            inputItems.Add<ItemChiliPepper>(count: 2);

            outputItems.Add<ItemChiliBeans>(count: 1);
        }
    }
}