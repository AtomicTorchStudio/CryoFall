namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeCampFuelSynthetic : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemCanisterPetroleum>(count: 2);
            inputItems.Add<ItemAcidNitric>(count: 1);

            outputItems.Add<ItemCampFuel>(count: 5);
            outputItems.Add<ItemCanisterEmpty>(count: 2);
            outputItems.Add<ItemBottleEmpty>(count: 1);

            this.Icon = ClientItemIconHelper.CreateComposedIcon(
                name: this.Id + "Icon",
                primaryIcon: GetItem<ItemCampFuel>().Icon,
                secondaryIcon: GetItem<ItemCanisterPetroleum>().Icon);
        }
    }
}