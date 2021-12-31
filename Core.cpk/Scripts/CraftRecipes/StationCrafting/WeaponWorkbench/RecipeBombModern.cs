namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBombModern : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWeaponWorkbench>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemExplosives>(count: 250);
            inputItems.Add<ItemIngotSteel>(count: 25);
            inputItems.Add<ItemComponentsElectronic>(count: 10);
            inputItems.Add<ItemComponentsWeapon>(count: 1);

            outputItems.Add<ItemBombModern>(count: 1);
        }
    }
}