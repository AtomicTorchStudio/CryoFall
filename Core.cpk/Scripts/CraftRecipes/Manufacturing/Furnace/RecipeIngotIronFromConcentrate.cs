namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeIngotIronFromConcentrate : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectFurnace>()
                    .Add<ObjectFurnaceElectric>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemOreIronConcentrate>(count: 1);

            outputItems.Add<ItemIngotIron>();

            this.Icon = ClientItemIconHelper.CreateComposedIcon(
                name: this.Id + "Icon",
                primaryIcon: GetItem<ItemIngotIron>().Icon,
                secondaryIcon: GetItem<ItemOreIronConcentrate>().Icon);
        }
    }
}