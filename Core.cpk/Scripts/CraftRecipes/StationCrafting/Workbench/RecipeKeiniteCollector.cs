namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeKeiniteCollector : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWorkbench>();

            duration = CraftingDuration.Long;

            inputItems.Add<ItemIngotSteel>(count: 10);
            inputItems.Add<ItemComponentsHighTech>(count: 5);
            inputItems.Add<ItemBatteryDisposable>(count: 5);
            inputItems.Add<ItemKeinite>(count: 1);

            outputItems.Add<ItemKeiniteCollector>();
        }
    }
}