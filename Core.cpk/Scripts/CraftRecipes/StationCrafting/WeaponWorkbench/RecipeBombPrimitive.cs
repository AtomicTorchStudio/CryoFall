namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeBombPrimitive : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWeaponWorkbench>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemBlackpowder>(count: 500);
            inputItems.Add<ItemIngotCopper>(count: 50);
            inputItems.Add<ItemPaper>(count: 100);
            inputItems.Add<ItemOilpod>(count: 20);
            inputItems.Add<ItemRubberVulcanized>(count: 10);

            outputItems.Add<ItemBombPrimitive>(count: 1);
        }
    }
}