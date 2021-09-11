namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Vehicle;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using System;

    public class RecipeVehicleKeiniteArtillery : Recipe.RecipeForStationCrafting
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
            inputItems.Add<ItemVialBiomaterial>(count: 20);
            inputItems.Add<ItemToxin>(count: 50);
            inputItems.Add<ItemInsectMeatRaw>(count: 5);

            outputItems.Add<ItemVehicleKeiniteArtillery>(count: 1);
        }
    }
}