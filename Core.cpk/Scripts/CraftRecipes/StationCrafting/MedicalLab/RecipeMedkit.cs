namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeMedkit : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMedicalLab>();

            duration = CraftingDuration.Medium;

            inputItems.Add<ItemComponentsPharmaceutical>(count: 10);
            inputItems.Add<ItemBandage>(count: 1);
            inputItems.Add<ItemHerbGreen>(count: 2);
            inputItems.Add<ItemHerbRed>(count: 2);
            inputItems.Add<ItemHerbPurple>(count: 2);

            outputItems.Add<ItemMedkit>(count: 1);
        }
    }
}