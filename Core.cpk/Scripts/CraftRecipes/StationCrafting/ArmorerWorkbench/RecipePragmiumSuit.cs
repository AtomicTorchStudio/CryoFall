namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.PragmiumArmor;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipePragmiumSuit : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectArmorerWorkbench>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemOrePragmium>(count: 100);
            inputItems.Add<ItemIngotSteel>(count: 20);
            inputItems.Add<ItemAramidFiber>(count: 20);
            inputItems.Add<ItemComponentsHighTech>(count: 10);
            inputItems.Add<ItemBallisticPlate>(count: 5);
            
            outputItems.Add<ItemPragmiumSuit>();
        }
    }
}