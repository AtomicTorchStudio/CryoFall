namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipePasta : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan craftDuration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCookingTable>();

            craftDuration = CraftingDuration.VeryShort;

            inputItems.Add<ItemWheatFlour>(count: 2);
            inputItems.Add<ItemSalt>(count: 1);
            inputItems.Add<ItemBottleWater>(count: 1);

            outputItems.Add<ItemPasta>(count: 1);
            outputItems.Add<ItemBottleEmpty>(count: 1);
        }
    }
}