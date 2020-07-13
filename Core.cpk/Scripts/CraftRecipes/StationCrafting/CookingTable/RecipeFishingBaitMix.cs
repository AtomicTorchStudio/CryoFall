namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Fishing;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using System;

    public class RecipeFishingBaitMix : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan craftDuration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCookingTable>();

            craftDuration = CraftingDuration.VeryShort;

            inputItems.Add<ItemDough>(count: 3);
            inputItems.Add<ItemMeatRaw>(count: 1);
            inputItems.Add<ItemFishFilletWhite>(count: 1);

            outputItems.Add<ItemFishingBaitMix>(count: 10);
        }
    }
}