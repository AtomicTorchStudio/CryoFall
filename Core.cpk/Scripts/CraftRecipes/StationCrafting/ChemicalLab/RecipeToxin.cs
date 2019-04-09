namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeToxin : Recipe.RecipeForStationCrafting
    {
        public override string Name => "Toxin extraction";

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMedicalLab>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemMushroomPink>(count: 1);
            inputItems.Add<ItemBottleWater>(count: 1);

            outputItems.Add<ItemToxin>(count: 2);
            outputItems.Add<ItemBottleEmpty>(count: 1);
        }
    }
}