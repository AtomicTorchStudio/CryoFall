namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeMREImprovised : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCookingTable>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemMeatCharred>(count: 1);
            inputItems.Add<ItemRoastedMushrooms>(count: 1);
            inputItems.Add<ItemSalt>(count: 1);

            outputItems.Add<ItemMREImprovised>();
        }
    }
}