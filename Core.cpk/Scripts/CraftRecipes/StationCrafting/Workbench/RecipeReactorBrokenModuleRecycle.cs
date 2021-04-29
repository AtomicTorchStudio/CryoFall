namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Reactor;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeReactorBrokenModuleRecycle : Recipe.RecipeForStationCrafting
    {
        public override string Name => "Reactor components recycle";

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWorkbench>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemReactorBrokenModule>(count: 5);

            outputItems.Add<ItemComponentsElectronic>(count: 1);
            outputItems.Add<ItemIngotSteel>(count: 5);
            outputItems.Add<ItemWire>(count: 5);

            this.Icon = ClientItemIconHelper.CreateComposedIcon(
            name: this.Id + "Icon",
            primaryIcon: GetItem<ItemComponentsElectronic>().Icon,
            secondaryIcon: GetItem<ItemReactorBrokenModule>().Icon);
        }
    }
}