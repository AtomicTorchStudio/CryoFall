namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.ApartSuit;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeApartSuit : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectArmorerWorkbench>();

            duration = CraftingDuration.Long;

            inputItems.Add<ItemIngotSteel>(count: 25);
            inputItems.Add<ItemAramidFiber>(count: 40);
            inputItems.Add<ItemTarpaulin>(count: 50);
            inputItems.Add<ItemBallisticPlate>(count: 2);
            inputItems.Add<ItemGlassRaw>(count: 50);
            inputItems.Add<ItemComponentsElectronic>(count: 10);

            outputItems.Add<ItemApartSuit>();
        }
    }
}