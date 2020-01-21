namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.PragmiumArmor;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipePragmiumArmor : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectArmorerWorkbench>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemOrePragmium>(count: 60);
            inputItems.Add<ItemIngotSteel>(count: 25);
            inputItems.Add<ItemAramidFiber>(count: 25);
            inputItems.Add<ItemBallisticPlate>(count: 3);
            inputItems.Add<ItemComponentsElectronic>(count: 10);

            outputItems.Add<ItemPragmiumArmor>();
        }
    }
}