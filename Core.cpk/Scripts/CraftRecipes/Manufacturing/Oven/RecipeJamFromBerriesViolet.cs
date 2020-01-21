namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeJamFromBerriesViolet : Recipe.RecipeForManufacturing
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectStove>();
            stations.Add<ObjectStoveElectric>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemBerriesViolet>(count: 3);
            inputItems.Add<ItemSugar>(count: 3);

            outputItems.Add<ItemJamBerries>();

            this.Icon = ClientItemIconHelper.CreateComposedIcon(
                name: this.Id + "Icon",
                primaryIcon: GetItem<ItemJamBerries>().Icon,
                secondaryIcon: GetItem<ItemBerriesViolet>().Icon);
        }
    }
}