namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using System;

    public class RecipeCanisterGasolineFromFuelSack : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemFuelSack>(count: 1);
            inputItems.Add<ItemCharcoal>(count: 1);
            inputItems.Add<ItemCanisterEmpty>(count: 1);

            outputItems.Add<ItemCanisterGasoline>(count: 1);
            outputItems.Add<ItemSlime>(count: 1);

            this.Icon = ClientItemIconHelper.CreateComposedIcon(
                name: this.Id + "Icon",
                primaryIcon: GetItem<ItemCanisterGasoline>().Icon,
                secondaryIcon: GetItem<ItemFuelSack>().Icon);
        }
    }
}