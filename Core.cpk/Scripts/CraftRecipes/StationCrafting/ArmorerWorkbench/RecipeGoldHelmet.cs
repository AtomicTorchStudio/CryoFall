namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeGoldHelmet : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectArmorerWorkbench>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemIngotGold>(count: 20);
            inputItems.Add<ItemGemDiamond>(count: 1);
            inputItems.Add<ItemGemEmerald>(count: 1);
            inputItems.Add<ItemGemRuby>(count: 1);
            inputItems.Add<ItemGemSapphire>(count: 1);
            inputItems.Add<ItemGemTopaz>(count: 1);
            inputItems.Add<ItemGemTourmaline>(count: 1);

            outputItems.Add<ItemGoldHelmet>();
        }
    }
}