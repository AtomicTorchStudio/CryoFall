namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeFertilizer : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectFarmingWorkbench>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemPotassiumNitrate>(count: 25);
            inputItems.Add<ItemAsh>(count: 25);
            inputItems.Add<ItemMulch>(count: 5);

            outputItems.Add<ItemFertilizer>(count: 5);
        }
    }
}