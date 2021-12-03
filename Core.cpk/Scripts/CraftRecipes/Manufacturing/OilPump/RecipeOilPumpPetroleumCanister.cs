namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class RecipeOilPumpPetroleumCanister
        : BaseRecipeRemoveLiquid<
            ItemCanisterEmpty,
            ItemCanisterPetroleum>
    {
        public override bool IsAutoUnlocked => true;

        public override string Name => "Fill canister with raw petroleum oil from oil pump";

        /// <summary>
        /// We don't want any interruption of oil pump (otherwise its IsActive state will become off for a fraction of a second)
        /// </summary>
        protected override TimeSpan CraftDuration => TimeSpan.Zero;

        protected override LiquidContainerState GetLiquidState(IStaticWorldObject staticWorldObject)
        {
            return ProtoObjectExtractor.GetPrivateState(staticWorldObject)
                                       .LiquidContainerState;
        }

        protected override void ServerOnLiquidAmountChanged(IStaticWorldObject objectManufacturer)
        {
            // do nothing
        }

        protected override void SetupRecipeStations(StationsList stations)
        {
            stations.AddAll<ProtoObjectOilPump>();
        }
    }
}