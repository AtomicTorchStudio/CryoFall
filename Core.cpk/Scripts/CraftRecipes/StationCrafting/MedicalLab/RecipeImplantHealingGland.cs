﻿namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeImplantHealingGland : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMedicalLab>();

            duration = CraftingDuration.Long;

            inputItems.Add<ItemComponentsPharmaceutical>(count: 20);
            inputItems.Add<ItemKeinite>(count: 10);
            inputItems.Add<ItemVialBiomaterial>(count: 10);
            inputItems.Add<ItemComponentsIndustrialChemicals>(count: 5);

            outputItems.Add<ItemImplantHealingGland>(count: 1);
        }
    }
}