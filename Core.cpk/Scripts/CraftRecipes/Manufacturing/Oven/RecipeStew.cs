namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeStew : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectStove>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemMeatRaw>(count: 5);
            inputItems.Add<ItemRoastedMushrooms>(count: 3);
            inputItems.Add<ItemTomato>(count: 3);
            inputItems.Add<ItemChiliPepper>(count: 1);

            outputItems.Add<ItemStew>(count: 3);
        }
    }
}