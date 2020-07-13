namespace AtomicTorch.CBND.CoreMod.CraftRecipes.MineralProcessingPlant
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeSandFromStone : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMineralProcessingPlant>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemStone>(count: 25);

            outputItems.Add<ItemSand>(count: 25);

            this.Icon = ClientItemIconHelper.CreateComposedIcon(
                name: this.Id + "Icon",
                primaryIcon: GetItem<ItemSand>().Icon,
                secondaryIcon: GetItem<ItemStone>().Icon);
        }
    }
}