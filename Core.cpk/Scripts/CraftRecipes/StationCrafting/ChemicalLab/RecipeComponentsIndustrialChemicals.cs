namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeComponentsIndustrialChemicals : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemPotassiumNitrate>(count: 25);
            inputItems.Add<ItemSulfurPowder>(count: 25);
            inputItems.Add<ItemSalt>(count: 25);
            inputItems.Add<ItemOreLithium>(count: 25);
            inputItems.Add<ItemAsh>(count: 25);
            inputItems.Add<ItemCanisterMineralOil>(count: 5);

            outputItems.Add<ItemComponentsIndustrialChemicals>(count: 25);
            outputItems.Add<ItemCanisterEmpty>(count: 5);
        }
    }
}