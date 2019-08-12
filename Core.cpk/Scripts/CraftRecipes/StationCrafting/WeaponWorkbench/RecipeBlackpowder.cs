namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBlackpowder : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWeaponWorkbench>();

            duration = CraftingDuration.VeryShort;

            //not exactly realistic recipe (should be 70%/20%/10%), but good for the game balance :)
            inputItems.Add<ItemPotassiumNitrate>(count: 10);
            inputItems.Add<ItemCharcoal>(count: 5);
            inputItems.Add<ItemSulfurPowder>(count: 5);

            outputItems.Add<ItemBlackpowder>(count: 10);
        }
    }
}