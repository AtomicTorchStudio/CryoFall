namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeRoastedMushroomsPennyBuns : Recipe.RecipeForManufacturing
    {
        public override bool IsAutoUnlocked => true;

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectCampfire>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemMushroomPennyBun>(count: 1);

            outputItems.Add<ItemRoastedMushrooms>();

            this.Icon = ClientItemIconHelper.CreateComposedIcon(
                name: this.Id + "Icon",
                primaryIcon: GetItem<ItemRoastedMushrooms>().Icon,
                secondaryIcon: GetItem<ItemMushroomPennyBun>().Icon);
        }
    }
}