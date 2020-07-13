namespace AtomicTorch.CBND.CoreMod.CraftRecipes.OilCrackingPlant
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class RecipeOilCrackingPlantGasolineCanister
        : BaseRecipeRemoveLiquid
            <ItemCanisterEmpty,
                ItemCanisterGasoline>
    {
        public sealed override bool IsAutoUnlocked => true;

        [NotLocalizable]
        public override string Name => "Fill canister with gasoline fuel from mineral oil processor";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;

        protected sealed override LiquidContainerState GetLiquidState(IStaticWorldObject objectManufacturer)
        {
            return GetPrivateState(objectManufacturer).LiquidStateGasoline;
        }

        protected override void ServerOnLiquidAmountChanged(IStaticWorldObject objectManufacturer)
        {
            var privateState = GetPrivateState(objectManufacturer);
            privateState.IsLiquidStatesChanged = true;
        }

        protected override void SetupRecipeStations(StationsList stations)
        {
            stations.AddAll<ProtoObjectOilCrackingPlant>();
        }

        private static ProtoObjectOilCrackingPlant.PrivateState GetPrivateState(IStaticWorldObject objectManufacturer)
        {
            return ProtoObjectOilCrackingPlant.GetPrivateState(objectManufacturer);
        }
    }
}