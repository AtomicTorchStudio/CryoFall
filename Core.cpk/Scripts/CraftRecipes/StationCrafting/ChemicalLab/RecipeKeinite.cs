namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeKeinite : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemKeiniteRaw>(count: 25);
            inputItems.Add<ItemSolvent>(count: 5);
            inputItems.Add<ItemAcidSulfuric>(count: 1);

            outputItems.Add<ItemKeinite>(count: 5);
            outputItems.Add<ItemBottleEmpty>(count: 1);
        }
    }
}