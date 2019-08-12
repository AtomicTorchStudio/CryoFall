namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeIngotGold : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectFurnace>()
                    .Add<ObjectFurnaceElectric>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemGoldNugget>(count: 10);
            inputItems.Add<ItemSalt>(count: 25);
            inputItems.Add<ItemPotassiumNitrate>(count: 10);

            outputItems.Add<ItemIngotGold>();
        }
    }
}