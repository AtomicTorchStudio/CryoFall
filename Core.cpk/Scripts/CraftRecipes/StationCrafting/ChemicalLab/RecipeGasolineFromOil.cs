namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeGasolineFromOil : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.Long;

            inputItems.Add<ItemCanisterMineralOil>(count: 10);
            inputItems.Add<ItemIngotGold>(count: 5);
            inputItems.Add<ItemBottleWater>(count: 20);

            outputItems.Add<ItemCanisterGasoline>(count: 10);
            outputItems.Add<ItemIngotGold>(count: 5);
            outputItems.Add<ItemBottleEmpty>(count: 20);

            this.Icon = ClientItemIconHelper.CreateComposedIcon(
                name: this.Id + "Icon",
                primaryIcon: GetItem<ItemCanisterGasoline>().Icon,
                secondaryIcon: GetItem<ItemCanisterMineralOil>().Icon);
        }
    }
}