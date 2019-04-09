namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeCoinShiny : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCoinMint>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemCoinPenny>(count: 10);
            inputItems.Add<ItemIngotLithium>(count: 1);
            inputItems.Add<ItemIngotGold>(count: 1);
            inputItems.Add<ItemFluxPowder>(count: 1);

            outputItems.Add<ItemCoinShiny>(count: 5);
        }
    }
}