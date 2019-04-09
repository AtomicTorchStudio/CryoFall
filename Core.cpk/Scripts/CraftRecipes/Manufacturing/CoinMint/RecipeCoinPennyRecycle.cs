namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Resources;

    public class RecipeCoinPennyRecycle : Recipe.RecipeForManufacturing
    {
        public override ITextureResource Icon => new TextureResource("Recipes/" + nameof(RecipeCoinPennyRecycle));

        public override string Name => "Recycle penny coin";

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCoinMint>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemCoinPenny>(count: 10);

            outputItems.Add<ItemIngotCopper>(count: 1);
            outputItems.Add<ItemIngotIron>(count: 1);
            outputItems.Add<ItemIngotSteel>(count: 1);
        }
    }
}