namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeNeuralRecombinator : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMedicalLab>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemComponentsPharmaceutical>(count: 25);
            inputItems.Add<ItemComponentsElectronic>(count: 25);
            inputItems.Add<ItemPlastic>(count: 25);
            inputItems.Add<ItemPowerCell>(count: 1);

            outputItems.Add<ItemNeuralRecombinator>(count: 1);
        }
    }
}