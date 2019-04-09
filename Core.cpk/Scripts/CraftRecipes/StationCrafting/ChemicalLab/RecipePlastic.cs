namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipePlastic : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemCanisterMineralOil>(count: 5);
            inputItems.Add<ItemCoal>(count: 1);

            outputItems.Add<ItemPlastic>(count: 5);
            outputItems.Add<ItemCanisterEmpty>(count: 5);
        }
    }
}