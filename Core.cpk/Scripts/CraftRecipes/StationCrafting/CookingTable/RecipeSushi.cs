﻿namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeSushi : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan craftDuration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCookingTable>();

            craftDuration = CraftingDuration.Short;

            inputItems.Add<ItemRiceCooked>(count: 2);
            inputItems.Add<ItemFishFilletRed>(count: 1);
            inputItems.Add<ItemFishFilletWhite>(count: 1);
            inputItems.Add<ItemSalt>(count: 1);
            inputItems.Add<ItemSpices>(count: 1);

            outputItems.Add<ItemSushi>(count: 2);
        }
    }
}