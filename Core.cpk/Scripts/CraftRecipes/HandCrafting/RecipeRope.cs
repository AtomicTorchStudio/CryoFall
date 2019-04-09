namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeRope : Recipe.RecipeForHandCrafting
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

            inputItems.Add<ItemFibers>(count: 20);

            outputItems.Add<ItemRope>(count: 1);
        }
    }
}