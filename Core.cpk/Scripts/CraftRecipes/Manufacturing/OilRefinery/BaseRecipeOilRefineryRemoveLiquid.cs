namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class BaseRecipeOilRefineryRemoveLiquid<TOutputProtoItem>
        : BaseRecipeRemoveLiquid
            <ItemCanisterEmpty,
                TOutputProtoItem>
        where TOutputProtoItem : class, IProtoItemLiquidStorage, new()
    {
        public sealed override bool IsAutoUnlocked => true;

        protected sealed override LiquidContainerState GetLiquidState(IStaticWorldObject objectManufacturer)
        {
            return this.GetLiquidState(GetPrivateState(objectManufacturer));
        }

        protected abstract LiquidContainerState GetLiquidState(ProtoObjectOilRefinery.PrivateState privateState);

        protected override void ServerOnLiquidAmountChanged(IStaticWorldObject objectManufacturer)
        {
            var privateState = GetPrivateState(objectManufacturer);
            privateState.IsLiquidStatesChanged = true;
        }

        protected override void SetupRecipeStations(StationsList stations)
        {
            stations.AddAll<ProtoObjectOilRefinery>();
        }

        private static ProtoObjectOilRefinery.PrivateState GetPrivateState(IStaticWorldObject objectManufacturer)
        {
            return ProtoObjectOilRefinery.GetPrivateState(objectManufacturer);
        }
    }
}