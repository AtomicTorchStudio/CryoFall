namespace AtomicTorch.CBND.CoreMod.Systems.LiquidContainer
{
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class LiquidContainerSystem
    {
        public static void UpdateWithManufacturing(
            IStaticWorldObject worldObject,
            LiquidContainerState liquidContainerState,
            LiquidContainerConfig liquidContainerConfig,
            ManufacturingState manufacturingState,
            ManufacturingConfig manufacturingConfig,
            double deltaTime,
            bool isProduceLiquid,
            bool forceUpdateRecipe = false)
        {
            ManufacturingMechanic.UpdateRecipeOnly(
                worldObject,
                manufacturingState,
                manufacturingConfig,
                force: forceUpdateRecipe);

            var isUseRequested = manufacturingState.HasActiveRecipe;

            UpdateWithoutManufacturing(
                liquidContainerState,
                liquidContainerConfig,
                deltaTime,
                isProduceLiquid,
                isUseRequested,
                out var wasUsed);

            if (wasUsed)
            {
                ManufacturingMechanic.UpdateCraftingQueueOnly(
                    manufacturingState,
                    deltaTime);
            }
        }

        public static void UpdateWithoutManufacturing(
            LiquidContainerState state,
            LiquidContainerConfig config,
            double deltaTime,
            bool isProduceLiquid,
            bool isUseRequested,
            out bool wasUsed,
            bool resetAmountToZeroWhenNotEnoughToUse = false)
        {
            var amount = state.Amount;
            if (isProduceLiquid)
            {
                amount += config.AmountAutoIncreasePerSecond * deltaTime;
            }

            wasUsed = false;

            if (isUseRequested)
            {
                var amountToDeduct = config.AmountAutoDecreasePerSecondWhenUse * deltaTime;
                if (amount >= amountToDeduct)
                {
                    amount -= amountToDeduct;
                    wasUsed = true;
                }
                else if (resetAmountToZeroWhenNotEnoughToUse)
                {
                    amount = 0;
                }
            }

            amount = MathHelper.Clamp(amount, 0, config.Capacity);

            state.Amount = amount;
        }
    }
}