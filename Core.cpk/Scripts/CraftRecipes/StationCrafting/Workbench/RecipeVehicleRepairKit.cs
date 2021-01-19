namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using System;

    public class RecipeVehicleRepairKit : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWorkbench>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemIngotSteel>(count: 50);
            inputItems.Add<ItemStructuralPlating>(count: 5);
            inputItems.Add<ItemUniversalActuator>(count: 5);
            inputItems.Add<ItemComponentsHighTech>(count: 5);
            inputItems.Add<ItemBatteryHeavyDuty>(count: 1);

            outputItems.Add<ItemVehicleRepairKit>();
        }
    }
}