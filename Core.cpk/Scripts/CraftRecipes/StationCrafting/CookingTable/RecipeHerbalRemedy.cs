namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeHerbalRemedy : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCookingTable>();
            stations.Add<ObjectMedicalLab>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemHerbGreen>(count: 2);
            inputItems.Add<ItemTreebark>(count: 2);
            inputItems.Add<ItemMushroomRust>(count: 1);
            inputItems.Add<ItemWaterbulb>(count: 1);

            outputItems.Add<ItemRemedyHerbal>(count: 1);
        }
    }
}