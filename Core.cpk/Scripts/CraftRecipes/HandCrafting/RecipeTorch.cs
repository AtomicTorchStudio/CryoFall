namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Lights;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeTorch : Recipe.RecipeForHandCrafting
    {
        public override bool IsAutoUnlocked => true;

        protected override void SetupRecipe(
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems,
            StationsList optionalStations)
        {
            optionalStations.Add<ObjectWorkbench>();

            duration = CraftingDuration.Second;

            inputItems.Add<ItemTwigs>(count: 5);
            inputItems.Add<ItemFibers>(count: 10);

            outputItems.Add<ItemTorch>(count: 1);
        }
    }
}