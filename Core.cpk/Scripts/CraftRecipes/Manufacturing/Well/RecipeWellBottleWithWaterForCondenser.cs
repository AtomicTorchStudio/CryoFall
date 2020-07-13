namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class RecipeWellBottleWithWaterForCondenser
        : BaseRecipeRemoveLiquid
            <ItemBottleEmpty,
                ItemBottleWater>
    {
        public override bool IsAutoUnlocked => true;

        [NotLocalizable]
        public override string Name => "Fill bottle with water from water condenser";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;

        public override bool CanBeCrafted(
            IWorldObject objectManufacturer,
            CraftingQueue craftingQueue,
            ushort countToCraft)
        {
            // Please note: no biome check here.
            return base.CanBeCrafted(objectManufacturer, craftingQueue, countToCraft);
        }

        protected override LiquidContainerState GetLiquidState(IStaticWorldObject objectManufacturer)
        {
            return ProtoObjectWell.GetPrivateState(objectManufacturer)
                                  .LiquidStateWater;
        }

        protected override void ServerOnLiquidAmountChanged(IStaticWorldObject objectManufacturer)
        {
            // do nothing
        }

        protected override void SetupRecipeStations(StationsList stations)
        {
            stations.AddAll<ObjectWaterCondenser>();
        }
    }
}