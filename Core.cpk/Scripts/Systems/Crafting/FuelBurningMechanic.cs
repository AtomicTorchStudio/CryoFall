namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
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
        /// Updates fuel burning state.
        /// </summary>
        /// <param name="objectManufacturer">Instance of world object performing manufacturing.</param>
        /// <param name="state">Instance of fuel burning state.</param>
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
            double byproductsQueueRate,
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
                if (byproductsCraftQueue is null)
                {
                    throw new Exception("No byproductsCraftQueue");
                }

                CraftingMechanics.ServerUpdate(byproductsCraftQueue,
                                               deltaTime * byproductsQueueRate);
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

            var currentByproductRecipe = byproductsCraftQueue.QueueItems.FirstOrDefault()?.RecipeEntry.Recipe
                                             as Recipe.RecipeForManufacturingByproduct;
            var newByproductRecipe = config.MatchRecipeForByproduct(state.CurrentFuelItemType);
            if (currentByproductRecipe == newByproductRecipe)
            {
                return;
            }

            byproductsCraftQueue.Clear();

            if (newByproductRecipe is not null)
            {
                CraftingMechanics.ServerStartCrafting(
                    objectManufacturer,
                    null,
                    byproductsCraftQueue,
                    new RecipeWithSkin(newByproductRecipe),
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
                || !isNeedFuelNow
                || state.ContainerFuel.OccupiedSlotsCount == 0)
            {
                return;
            }

            // look for the fuel item with the lowest fuel value
            IItem fuelItem = null;
            var lowestFuelItemValue = double.MaxValue;
            foreach (var item in state.ContainerFuel.Items)
            {
                if (item.ProtoItem is IProtoItemFuelSolid fuelItemType)
                {
                    var itemFuelAmount = fuelItemType.FuelAmount;
                    if (itemFuelAmount < lowestFuelItemValue)
                    {
                        fuelItem = item;
                        lowestFuelItemValue = itemFuelAmount;
                    }
                }
            }

            var bestProtoFuel = fuelItem?.ProtoGameObject as IProtoItemFuelSolid;
            if (bestProtoFuel is null)
            {
                // no fuel placed in fuel container
                //if (state.CurrentFuelItemType is not null)
                //{
                //    state.CurrentFuelItemType = null;
                //    Logger.Info($"Fuel depleted for manufacturing at {objectManufacturer}");
                //}

                return;
            }

            Logger.Info($"Fuel will be used for manufacturing {bestProtoFuel.ShortId} at {objectManufacturer}");

            // destroy one fuel item
            Api.Server.Items.SetCount(fuelItem, fuelItem.Count - 1);

            // set fuel burn time
            state.CurrentFuelItemType = bestProtoFuel;
            var secondsToBurn = bestProtoFuel.FuelAmount;
            state.FuelUseTimeRemainsSeconds = secondsToBurn;

            OnFuelChanged(objectManufacturer, state, byproductsCraftQueue, config);
        }
    }
}