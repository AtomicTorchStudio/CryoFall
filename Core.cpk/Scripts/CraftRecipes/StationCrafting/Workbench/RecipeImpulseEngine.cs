namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeImpulseEngine : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWorkbench>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemIngotCopper>(count: 10);
            inputItems.Add<ItemComponentsElectronic>(count: 5);
            inputItems.Add<ItemIngotLithium>(count: 5);
            inputItems.Add<ItemOrePragmium>(count: 5);

            outputItems.Add<ItemImpulseEngine>(count: 1);
        }
    }
}