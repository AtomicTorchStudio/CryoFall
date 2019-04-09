namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBatteryHeavyDuty : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.Long;

            inputItems.Add<ItemIngotCopper>(count: 40);
            inputItems.Add<ItemPlastic>(count: 5);
            inputItems.Add<ItemIngotLithium>(count: 5);
            inputItems.Add<ItemComponentsIndustrialChemicals>(count: 5);

            outputItems.Add<ItemBatteryHeavyDuty>(count: 10);
        }
    }
}