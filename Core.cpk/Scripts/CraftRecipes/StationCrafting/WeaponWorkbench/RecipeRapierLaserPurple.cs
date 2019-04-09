namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class RecipeRapierLaserPurple : Recipe.RecipeForStationCrafting
    {
        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectWeaponWorkbench>();

            duration = CraftingDuration.VeryLong;

            inputItems.Add<ItemIngotCopper>(count: 50);
            inputItems.Add<ItemPlastic>(count: 20);
            inputItems.Add<ItemComponentsOptical>(count: 10);
            inputItems.Add<ItemOrePragmium>(count: 10);
            inputItems.Add<ItemGemTourmaline>(count: 3);
            inputItems.Add<ItemPowerCell>(count: 1);

            outputItems.Add<ItemRapierLaserPurple>();
        }
    }
}