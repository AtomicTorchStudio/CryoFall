namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeOreCopperConcentrate : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMineralProcessingPlant>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemOreCopper>(count: 3);

            outputItems.Add<ItemOreCopperConcentrate>(count: 1);
            outputItems.Add<ItemSand>(count: 1);
        }
    }
}