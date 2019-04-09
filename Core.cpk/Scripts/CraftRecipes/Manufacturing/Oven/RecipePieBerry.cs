namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipePieBerry : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectStove>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemWheatFlour>(count: 5);
            inputItems.Add<ItemJamBerries>(count: 3);
            inputItems.Add<ItemSugar>(count: 3);
            inputItems.Add<ItemEggsRaw>(count: 1);

            outputItems.Add<ItemPieBerry>(count: 3);
        }
    }
}