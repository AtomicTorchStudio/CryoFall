namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeUniversalActuator : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWorkbench>();

            duration = CraftingDuration.Medium;

            inputItems
                .Add<ItemIngotSteel>(count: 5)
                .Add<ItemPlastic>(count: 5)
                .Add<ItemCanisterMineralOil>(count: 1)
                .Add<ItemComponentsMechanical>(count: 5);

            outputItems
                .Add<ItemUniversalActuator>(count: 1)
                .Add<ItemCanisterEmpty>(count: 1);
        }
    }
}