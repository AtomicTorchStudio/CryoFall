namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;

    public class RecipeOilRefineryMineralOilCanister : BaseRecipeOilRefineryRemoveLiquid<ItemCanisterMineralOil>
    {
        public override string Name => "Fill canister with mineral oil from oil refinery";

        protected override TimeSpan CraftDuration => CraftingDuration.Instant;

        protected override LiquidContainerState GetLiquidState(ProtoObjectOilRefinery.PrivateState privateState)
        {
            return privateState.LiquidStateMineralOil;
        }
    }
}