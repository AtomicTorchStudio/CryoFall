namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeCarbonara : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectStove>();
            stations.Add<ObjectStoveElectric>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemPasta>(count: 2);
            inputItems.Add<ItemEggsRaw>(count: 1);
            inputItems.Add<ItemMeatRoasted>(count: 1);
            inputItems.Add<ItemCheese>(count: 1);

            outputItems.Add<ItemCarbonara>(count: 2);
        }
    }
}