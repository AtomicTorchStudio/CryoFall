namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
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

            inputItems.Add<ItemGlassRaw>(count: 25);
            inputItems.Add<ItemWire>(count: 25);
            inputItems.Add<ItemPlastic>(count: 25);
            inputItems.Add<ItemComponentsOptical>(count: 20);
            inputItems.Add<ItemComponentsElectronic>(count: 10);

            outputItems.Add<ItemSolarPanel>(count: 1);
        }
    }
}