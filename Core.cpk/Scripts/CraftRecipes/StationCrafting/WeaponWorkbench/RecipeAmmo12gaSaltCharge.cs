namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeAmmo12gaSaltCharge : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWeaponWorkbench>();

            duration = CraftingDuration.VeryShort;

            inputItems.Add<ItemIngotCopper>(count: 2);
            inputItems.Add<ItemPaper>(count: 10);
            inputItems.Add<ItemBlackpowder>(count: 15);
            inputItems.Add<ItemSalt>(count: 10);

            outputItems.Add<ItemAmmo12gaSaltCharge>(count: 20);
        }
    }
}