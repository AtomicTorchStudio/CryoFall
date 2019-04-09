namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeFillBottleWithWaterbulb : Recipe.RecipeForHandCrafting
    {
        public override bool IsAutoUnlocked => false;

        public override string Name => "Fill bottle with waterbulb";

        protected override void SetupRecipe(
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems,
            StationsList optionalStations)
        {
            optionalStations.Add<ObjectWorkbench>();

            duration = CraftingDuration.Second;

            inputItems.Add<ItemWaterbulb>(count: 5);
            inputItems.Add<ItemBottleEmpty>(count: 1);

            outputItems.Add<ItemBottleWater>(count: 1);
        }
    }
}