namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using System;

    public class RecipeAmmoArrowStone : Recipe.RecipeForHandCrafting
    {
        protected override void SetupRecipe(
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems,
            StationsList optionalStations)
        {
            optionalStations.Add<ObjectWeaponWorkbench>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemTwigs>(count: 20);
            inputItems.Add<ItemFibers>(count: 20);
            inputItems.Add<ItemStone>(count: 5);

            outputItems.Add<ItemAmmoArrowStone>(count: 20);
        }
    }
}