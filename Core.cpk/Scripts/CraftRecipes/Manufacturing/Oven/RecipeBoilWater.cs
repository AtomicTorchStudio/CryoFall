namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBoilWater : Recipe.RecipeForManufacturing
    {
        public override bool IsAutoUnlocked => true;

        public override string Name => "Boil water";

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectStove>();
            stations.Add<ObjectStoveElectric>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemBottleWaterStale>(count: 1);

            outputItems.Add<ItemBottleWater>();
        }
    }
}