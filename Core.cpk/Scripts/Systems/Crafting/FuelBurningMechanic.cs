namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// Mechanic for fuel burning (in special buildings).
    /// </summary>
    public static class FuelBurningMechanic
    {
        private static readonly ILogger Logger = Api.Logger;

        /// <summary>
        /// Updates manufacturing state.
        /// </summary>
        /// <param name="objectManufacturer">Instance of world object performing manufacturing.</param>
        /// <param name="state">Instance of manufacturing state.</param>
        /// <param name="byproductsCraftQueue"></param>
        /// <param name="config">Manufacturing config.</param>
        /// <param name="deltaTime">Delta time to progress on.</param>
        /// <param name="isNeedFuelNow">The new fuel item will be not burned if the fuel is not needed now.</param>
        public static void Update(
            IStaticWorldObject objectManufacturer,
            FuelBurningState state,
            CraftingQueue byproductsCraftQueue,
            ManufacturingConfig config,
            double deltaTime,
            bool isNeedFuelNow,
            bool forceRefreshFuel = false)
        {
            if (isNeedFuelNow
                && (state.ContainerFuel.StateHash != state.ContainerFuelLastStateHash
                    || forceRefreshFuel))
            {
                RefreshFuel(state, byproductsCraftQueue, config, objectManufacturer, isNeedFuelNow);
                state.ContainerFuelLastStateHash = state.ContainerFuel.StateHash;
            }

            var fuelUseTimeRemainsSeconds =
                state.FuelUseTimeRemainsSeconds
                + state.FuelUseTimeAccumulatedRemainder
                - deltaTime;

            if (fuelUseTimeRemainsSeconds <= 0)
            {
                // fuel is burned
                state.FuelUseTimeAccumulatedRemainder += state.FuelUseTimeRemainsSeconds;
                state.FuelUseTimeRemainsSeconds = 0;

                if (isNeedFuelNow)
                {
                    // refresh fuel
                    RefreshFuel(state, byproductsCraftQueue, config, objectManufacturer, isNeedFuelNow);
                    return;
                }

                return;
            }

            // subtract fuel burn time
            state.FuelUseTimeAccumulatedRemainder = 0;
            state.FuelUseTimeRemainsSeconds = fuelUseTimeRemainsSeconds;

            if (config.IsProduceByproducts)
            {
                if (byproductsCraftQueue == null)
                {
                    throw new Exception("No byproductsCraftQueue");
                }

                CraftingMechanics.ServerUpdate(byproductsCraftQueue, deltaTime);
            }
        }

        private static void OnFuelChanged(
            IStaticWorldObject objectManufacturer,
            FuelBurningState state,
            CraftingQueue byproductsCraftQueue,
            ManufacturingConfig config)
        {
            if (!config.IsProduceByproducts)
            {
                return;
            }

            var currentByproductRecipe = byproductsCraftQueue.QueueItems.FirstOrDefault()?.Recipe
                                             as Recipe.RecipeForManufacturingByproduct;
            var newByproductRecipe = config.MatchRecipeForByproduct(state.CurrentFuelItemType);
            if (currentByproductRecipe == newByproductRecipe)
            {
                return;
            }

            byproductsCraftQueue.Clear();

            if (newByproductRecipe != null)
            {
                CraftingMechanics.ServerStartCrafting(
                    objectManufacturer,
                    null,
                    byproductsCraftQueue,
                    newByproductRecipe,
                    // unlimited count
                    countToCraft: ushort.MaxValue);
            }
        }

        /// <summary>
        /// Consumes fuel and populate remaining fuel burning time when needed.
        /// </summary>
        private static void RefreshFuel(
            FuelBurningState state,
            CraftingQueue byproductsCraftQueue,
            ManufacturingConfig config,
            IStaticWorldObject objectManufacturer,
            bool isNeedFuelNow)
        {
            if (state.FuelUseTimeRemainsSeconds > 0
                || !isNeedFuelNow)
            {
                return;
            }

            // look for best fuel item from placed into the fuel container
            var bestFuelItem = state.ContainerFuel.Items.OrderBy(
                                        i =>
                                        {
                                            var fuelItemType = i.ProtoItem as IProtoItemFuelSolid;
                                            return fuelItemType?.FuelAmount ?? double.MaxValue;
                                        })
                                    .FirstOrDefault();

            var bestFuelItemType = bestFuelItem?.ProtoItem as IProtoItemFuelSolid;
            if (bestFuelItemType == null)
            {
                // no fuel placed in fuel container
                //if (state.CurrentFuelItemType != null)
                //{
                //    state.CurrentFuelItemType = null;
                //    Logger.Info($"Fuel depleted for manufacturing at {objectManufacturer}");
                //}

                return;
            }

            Logger.Info($"Fuel will be used for manufacturing {bestFuelItemType.ShortId} at {objectManufacturer}");

            // destroy one fuel item
            Api.Server.Items.SetCount(bestFuelItem, bestFuelItem.Count - 1);

            // set fuel burn time
            state.CurrentFuelItemType = bestFuelItemType;
            var secondsToBurn = bestFuelItemType.FuelAmount;
            state.FuelUseTimeRemainsSeconds = secondsToBurn;

            OnFuelChanged(objectManufacturer, state, byproductsCraftQueue, config);
        }
    }
}