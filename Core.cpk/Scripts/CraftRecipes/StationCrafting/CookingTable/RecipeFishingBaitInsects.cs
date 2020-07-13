namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Fishing;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using System;

    public class RecipeFishingBaitInsects : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan craftDuration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCookingTable>();

            craftDuration = CraftingDuration.VeryShort;

            inputItems.Add<ItemInsectMeatRaw>(count: 4);

            outputItems.Add<ItemFishingBaitInsect>(count: 10);
        }
    }
}