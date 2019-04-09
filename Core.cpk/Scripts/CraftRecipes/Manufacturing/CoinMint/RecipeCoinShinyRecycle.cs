namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Resources;

    public class RecipeCoinShinyRecycle : Recipe.RecipeForManufacturing
    {
        public override ITextureResource Icon => new TextureResource("Recipes/" + nameof(RecipeCoinShinyRecycle));

        public override string Name => "Recycle shiny coin";

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCoinMint>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemCoinShiny>(count: 5);

            outputItems.Add<ItemCoinPenny>(count: 10);
            outputItems.Add<ItemIngotLithium>(count: 1);
            outputItems.Add<ItemIngotGold>(count: 1);
        }
    }
}