namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;

    public class RecipeOilRefineryGasolineCanister : BaseRecipeOilRefineryRemoveLiquid<ItemCanisterGasoline>
    {
        public override string Name => "Fill canister with gasoline fuel from oil refinery";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;

        protected override LiquidContainerState GetLiquidState(ProtoObjectOilRefinery.PrivateState privateState)
        {
            return privateState.LiquidStateGasoline;
        }
    }
}