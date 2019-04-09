namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeAmmo300Incendiary : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWeaponWorkbench>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemIngotSteel>(count: 5);
            inputItems.Add<ItemIngotCopper>(count: 5);
            inputItems.Add<ItemFormulatedGunpowder>(count: 30);
            inputItems.Add<ItemComponentsIndustrialChemicals>(count: 5);

            outputItems.Add<ItemAmmo300Incendiary>(count: 10);
        }
    }
}