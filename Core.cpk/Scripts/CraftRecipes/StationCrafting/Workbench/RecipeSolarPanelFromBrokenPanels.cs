namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using System;

    public class RecipeSolarPanelFromBrokenPanels : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWorkbench>();

            duration = CraftingDuration.Long;

            inputItems.Add<ItemSolarPanelBroken>(count: 4);
            inputItems.Add<ItemComponentsOptical>(count: 10);
            inputItems.Add<ItemComponentsElectronic>(count: 10);
            inputItems.Add<ItemOrePragmium>(count: 10);

            outputItems.Add<ItemSolarPanel>(count: 1);

            this.Icon = ClientItemIconHelper.CreateComposedIcon(
                name: this.Id + "Icon",
                primaryIcon: GetItem<ItemSolarPanel>().Icon,
                secondaryIcon: GetItem<ItemSolarPanelBroken>().Icon);
        }
    }
}