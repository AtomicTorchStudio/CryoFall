namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class RecipeWellBottleWithWater
        : BaseRecipeRemoveLiquid
            <ItemBottleEmpty,
                ItemBottleWater>
    {
        public override bool IsAutoUnlocked => true;

        public override string Name => "Fill bottle with water from well";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;

        public override bool CanBeCrafted(
            IWorldObject objectManufacturer,
            CraftingQueue craftingQueue,
            ushort countToCraft)
        {
            if (((IStaticWorldObject)objectManufacturer).OccupiedTile.ProtoTile is IProtoTileWellAllowed protoTile
                && !protoTile.IsStaleWellWater)
            {
                return base.CanBeCrafted(objectManufacturer, craftingQueue, countToCraft);
            }

            return false;
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
            stations.AddAll<ProtoObjectWell>();
        }
    }
}