namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeNeuralEnhancer : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMedicalLab>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemComponentsPharmaceutical>(count: 100);
            inputItems.Add<ItemComponentsHighTech>(count: 5);
            inputItems.Add<ItemPowerCell>(count: 1);
            inputItems.Add<ItemPlastic>(count: 20);

            outputItems.Add<ItemNeuralEnhancer>(count: 1);
        }
    }
}