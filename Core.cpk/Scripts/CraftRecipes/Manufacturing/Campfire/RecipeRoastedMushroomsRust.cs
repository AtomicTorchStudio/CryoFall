namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeRoastedMushroomsRust : Recipe.RecipeForManufacturing
    {
        public override bool IsAutoUnlocked => true;

        public override string Name => "Roasted mushrooms (rust)";

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCampfire>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemMushroomRust>(count: 1);

            outputItems.Add<ItemRoastedMushrooms>();
        }
    }
}