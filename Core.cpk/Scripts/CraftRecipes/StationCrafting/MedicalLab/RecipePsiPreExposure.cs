namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipePsiPreExposure : Recipe.RecipeForStationCrafting
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
            inputItems.Add<ItemOreLithium>(count: 20);
            inputItems.Add<ItemHerbRed>(count: 1);
            inputItems.Add<ItemFlowerBlueSage>(count: 1);

            outputItems.Add<ItemPsiPreExposure>(count: 1);
        }
    }
}