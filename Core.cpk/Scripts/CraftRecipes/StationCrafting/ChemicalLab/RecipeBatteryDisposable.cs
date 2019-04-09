namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBatteryDisposable : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectChemicalLab>();

            duration = CraftingDuration.Long;

            inputItems.Add<ItemPaper>(count: 40);
            inputItems.Add<ItemSalt>(count: 25);
            inputItems.Add<ItemAsh>(count: 25);
            inputItems.Add<ItemIngotCopper>(count: 10);
            inputItems.Add<ItemCoal>(count: 5);

            outputItems.Add<ItemBatteryDisposable>(count: 10);
        }
    }
}