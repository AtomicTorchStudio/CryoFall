namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeJamBerriesOrange : Recipe.RecipeForManufacturing
    {
        public override string Name => "Berry jam (orange berries)";

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectStove>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemBerriesOrange>(count: 3);
            inputItems.Add<ItemSugar>(count: 3);

            outputItems.Add<ItemJamBerries>();
        }
    }
}