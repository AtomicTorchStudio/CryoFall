namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using System;

    public class RecipeDroneIndustrialAdvanced : Recipe.RecipeForStationCrafting
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
            inputItems.Add<ItemComponentsElectronic>(count: 5);
            inputItems.Add<ItemComponentsOptical>(count: 5);
            inputItems.Add<ItemPowerCell>(count: 1);

            outputItems.Add<ItemDroneIndustrialAdvanced>();
        }
    }
}