namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeStrengthBoostBig : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMedicalLab>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemCoffeeBeans>(count: 20);
            inputItems.Add<ItemToxin>(count: 20);
            inputItems.Add<ItemHerbRed>(count: 2);
            inputItems.Add<ItemBottleWater>(count: 1);

            outputItems.Add<ItemStrengthBoostBig>(count: 1);
        }
    }
}