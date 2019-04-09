namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeMetalChestplate : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectArmorerWorkbench>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemIngotIron>(count: 40);
            inputItems.Add<ItemIngotCopper>(count: 15);
            inputItems.Add<ItemFluxPowder>(count: 25);
            inputItems.Add<ItemLeather>(count: 2);
            inputItems.Add<ItemRope>(count: 2);

            outputItems.Add<ItemMetalChestplate>();
        }
    }
}