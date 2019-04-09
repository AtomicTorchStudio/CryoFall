namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeExplosives : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemNitrocellulosePowder>(count: 20);
            inputItems.Add<ItemSolvent>(count: 10);
            inputItems.Add<ItemAcidSulfuric>(count: 1);
            inputItems.Add<ItemPlastic>(count: 1);

            outputItems.Add<ItemExplosives>(count: 20);
        }
    }
}