﻿namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeAcidNitric : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.Second;

            inputItems.Add<ItemAcidSulfuric>(count: 2);
            inputItems.Add<ItemPotassiumNitrate>(count: 5);

            outputItems.Add<ItemAcidNitric>(count: 1);
            outputItems.Add<ItemBottleEmpty>(count: 1);
        }
    }
}