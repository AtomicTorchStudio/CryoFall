namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeSolarPanel : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWorkbench>();

            duration = CraftingDuration.Long;

            inputItems.Add<ItemGlassRaw>(count: 100);
            inputItems.Add<ItemPlastic>(count: 50);
            inputItems.Add<ItemWire>(count: 50);
            inputItems.Add<ItemComponentsOptical>(count: 50);
            inputItems.Add<ItemComponentsElectronic>(count: 40);
            inputItems.Add<ItemOrePragmium>(count: 10);
            inputItems.Add<ItemPowerCell>(count: 5);

            outputItems.Add<ItemSolarPanel>(count: 1);
        }
    }
}