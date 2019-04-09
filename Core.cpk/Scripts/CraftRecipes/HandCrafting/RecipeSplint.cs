namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeSplint : Recipe.RecipeForHandCrafting
    {
        public override bool IsAutoUnlocked => true;

        protected override void SetupRecipe(
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems,
            StationsList optionalStations)
        {
            optionalStations.Add<ObjectMedicalLab>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemRope>(count: 1);
            inputItems.Add<ItemTwigs>(count: 2);

            outputItems.Add<ItemSplint>(count: 1);
        }
    }
}