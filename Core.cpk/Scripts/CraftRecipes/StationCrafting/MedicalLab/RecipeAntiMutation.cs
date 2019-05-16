namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeAntiMutation : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMedicalLab>();
            stations.Add<ObjectCookingTable>();

            duration = CraftingDuration.Short;

            inputItems.Add<ItemHerbGreen>(count: 10);
            inputItems.Add<ItemHerbRed>(count: 5);
            inputItems.Add<ItemHerbPurple>(count: 5);
            inputItems.Add<ItemToxin>(count: 5);
            inputItems.Add<ItemSalt>(count: 5);

            outputItems.Add<ItemAntiMutation>(count: 1);
        }
    }
}