namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeComponentsHighTech : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWorkbench>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemComponentsElectronic>(count: 10);
            inputItems.Add<ItemIngotCopper>(count: 20);
            inputItems.Add<ItemIngotGold>(count: 4);
            inputItems.Add<ItemIngotLithium>(count: 4);

            outputItems.Add<ItemComponentsHighTech>(count: 5);
        }
    }
}