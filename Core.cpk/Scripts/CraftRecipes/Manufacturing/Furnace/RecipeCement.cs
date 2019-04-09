namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeCement : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectFurnace>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemClay>(count: 10);
            inputItems.Add<ItemSand>(count: 10);
            inputItems.Add<ItemStone>(count: 10);

            outputItems.Add<ItemCement>(count: 10);
        }
    }
}