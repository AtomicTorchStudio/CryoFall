namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBombResonance : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWeaponWorkbench>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemExplosives>(count: 300);
            inputItems.Add<ItemIngotSteel>(count: 50);
            inputItems.Add<ItemIngotLithium>(count: 50);
            inputItems.Add<ItemOrePragmium>(count: 50);
            inputItems.Add<ItemBatteryDisposable>(count: 20);
            inputItems.Add<ItemComponentsElectronic>(count: 20);

            outputItems.Add<ItemBombResonance>(count: 1);
        }
    }
}