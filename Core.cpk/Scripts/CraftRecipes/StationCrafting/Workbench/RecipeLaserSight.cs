namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeLaserSight : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWorkbench>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemIngotSteel>(count: 10);
            inputItems.Add<ItemGlassRaw>(count: 10);
            inputItems.Add<ItemWire>(count: 10);
            inputItems.Add<ItemBatteryDisposable>(count: 1);

            outputItems.Add<ItemLaserSight>();
        }
    }
}