namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Vehicle;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using System;

    public class RecipeVehicleSwarmLauncher : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemKeinite>(count: 50);
            inputItems.Add<ItemVialBiomaterial>(count: 10);
            inputItems.Add<ItemSlime>(count: 50);
            inputItems.Add<ItemInsectMeatRaw>(count: 10);

            outputItems.Add<ItemVehicleSwarmLauncher>(count: 1);
        }
    }
}