namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBiomaterialCollector : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMedicalLab>();

            duration = CraftingDuration.Long;

            inputItems.Add<ItemIngotSteel>(count: 20);
            inputItems.Add<ItemIngotCopper>(count: 10);
            inputItems.Add<ItemComponentsHighTech>(count: 10);
            inputItems.Add<ItemComponentsOptical>(count: 5);

            outputItems.Add<ItemBiomaterialCollector>(count: 1);
        }
    }
}