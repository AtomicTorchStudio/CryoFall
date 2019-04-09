namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.Quilted;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeQuiltedCoat : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectArmorerWorkbench>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemThread>(count: 25);
            inputItems.Add<ItemRope>(count: 5);
            inputItems.Add<ItemGlue>(count: 1);

            outputItems.Add<ItemQuiltedCoat>();
        }
    }
}