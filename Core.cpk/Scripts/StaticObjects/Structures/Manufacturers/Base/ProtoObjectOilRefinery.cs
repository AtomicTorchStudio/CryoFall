namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;

    public abstract class ProtoObjectOilRefinery
        : ProtoObjectManufacturer<
            ObjectOilRefineryPrivateState,
            ObjectManufacturerPublicState,
            StaticObjectClientState>
    {
        public override bool IsAutoSelectRecipe => true;

        public override bool IsFuelProduceByproducts => false;

        /// <summary>
        /// Capacity of gasoline in refinery.
        /// </summary>
        public abstract float LiquidCapacityGasoline { get; }

        /// <summary>
        /// Capacity of mineral oil in refinery.
        /// </summary>
        public abstract float LiquidCapacityMineralOil { get; }

        /// <summary>
        /// Capacity of raw petroleum in refinery.
        /// </summary>
        public abstract float LiquidCapacityRawPetroleum { get; }

        /// <summary>
        /// Gasoline production amount per second.
        /// </summary>
        public abstract float LiquidGasolineProductionPerSecond { get; }

        /// <summary>
        /// Mineral oil production amount per second.
        /// </summary>
        public abstract float LiquidMineralOilProductionPerSecond { get; }

        /// <summary>
        /// Raw petroleump consumption amount per second.
        /// </summary>
        public abstract float LiquidRawPetroleumConsumptionPerSecond { get; }

        protected LiquidContainerConfig LiquidConfigGasoline { get; private set; }

        protected LiquidContainerConfig LiquidConfigMineralOil { get; private set; }

        protected LiquidContainerConfig LiquidConfigRawPetroleum { get; private set; }

        protected ManufacturingConfig ManufacturingConfigGasoline { get; private set; }

        protected ManufacturingConfig ManufacturingConfigMineralOil { get; private set; }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            var privateState = data.SyncPrivateState;
            return WindowOilRefinery.Open(
                new ViewModelWindowOilRefinery(
                    data.GameObject,
                    privateState.ManufacturingState,
                    privateState.ManufacturingStateGasoline,
                    privateState.ManufacturingStateMineralOil,
                    this.ManufacturingConfig,
                    this.ManufacturingConfigGasoline,
                    this.ManufacturingConfigMineralOil,
                    privateState.FuelBurningState,
                    privateState.LiquidStateRawPetroleum,
                    privateState.LiquidStateGasoline,
                    privateState.LiquidStateMineralOil,
                    this.LiquidConfigRawPetroleum,
                    this.LiquidConfigGasoline,
                    this.LiquidConfigMineralOil));
        }

        protected override ManufacturingConfig PrepareManufacturingConfig()
        {
            var recipesForByproducts = FindProtoEntities<Recipe.RecipeForManufacturingByproduct>()
                .Where(r => r.StationTypes.Count == 0 || r.StationTypes.Contains(this));

            return new ManufacturingConfig(
                this,
                new List<Recipe>
                {
                    GetProtoEntity<RecipeOilRefineryEmptyCanisterFromPetroleumCanister>()
                },
                recipesForByproducts,
                isProduceByproducts: this.IsFuelProduceByproducts,
                isAutoSelectRecipe: this.IsAutoSelectRecipe);
        }

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.ManufacturingConfigGasoline = new ManufacturingConfig(
                this,
                new List<Recipe>
                {
                    GetProtoEntity<RecipeOilRefineryGasolineCanister>()
                },
                recipesForByproducts: null,
                isProduceByproducts: false,
                isAutoSelectRecipe: true);

            this.ManufacturingConfigMineralOil = new ManufacturingConfig(
                this,
                new List<Recipe>
                {
                    GetProtoEntity<RecipeOilRefineryMineralOilCanister>()
                },
                recipesForByproducts: null,
                isProduceByproducts: false,
                isAutoSelectRecipe: true);

            this.LiquidConfigRawPetroleum = new LiquidContainerConfig(
                capacity: this.LiquidCapacityRawPetroleum,
                // Not increased via "generation"
                // it's increased when a special recipe (petroleum canister->empty canister) is completed.
                amountAutoIncreasePerSecond: 0,
                amountAutoDecreasePerSecondWhenUse: this.LiquidRawPetroleumConsumptionPerSecond);

            this.LiquidConfigMineralOil = new LiquidContainerConfig(
                capacity: this.LiquidCapacityMineralOil,
                amountAutoIncreasePerSecond: this.LiquidMineralOilProductionPerSecond,
                // not decreased via "use", but via recipe
                amountAutoDecreasePerSecondWhenUse: 0);

            this.LiquidConfigGasoline = new LiquidContainerConfig(
                capacity: this.LiquidCapacityGasoline,
                amountAutoIncreasePerSecond: this.LiquidGasolineProductionPerSecond,
                // not decreased via "use", but via recipe
                amountAutoDecreasePerSecondWhenUse: 0);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            var privateState = data.PrivateState;

            // create liquid states
            if (privateState.LiquidStateRawPetroleum == null)
            {
                privateState.LiquidStateRawPetroleum = new LiquidContainerState();
            }

            if (privateState.LiquidStateGasoline == null)
            {
                privateState.LiquidStateGasoline = new LiquidContainerState();
            }

            if (privateState.LiquidStateMineralOil == null)
            {
                privateState.LiquidStateMineralOil = new LiquidContainerState();
            }

            // setup manufacturing state for gasoline
            var manufacturingStateProcessedGasoline = privateState.ManufacturingStateGasoline;
            if (manufacturingStateProcessedGasoline == null)
            {
                privateState.ManufacturingStateGasoline
                    = manufacturingStateProcessedGasoline
                          = new ManufacturingState(
                              data.GameObject,
                              containerInputSlotsCount: 1,
                              containerOutputSlotsCount: 1);
            }
            else
            {
                manufacturingStateProcessedGasoline.SetSlotsCount(input: 1, output: 1);
            }

            // setup manufacturing state for mineral oil
            var manufacturingStateProcessedMineralOil = privateState.ManufacturingStateMineralOil;
            if (manufacturingStateProcessedMineralOil == null)
            {
                privateState.ManufacturingStateMineralOil
                    = manufacturingStateProcessedMineralOil
                          = new ManufacturingState(
                              data.GameObject,
                              containerInputSlotsCount: 1,
                              containerOutputSlotsCount: 1);
            }
            else
            {
                manufacturingStateProcessedMineralOil.SetSlotsCount(input: 1, output: 1);
            }

            // setup input container types
            var itemsService = Server.Items;
            itemsService.SetContainerType<ItemsContainerLiquidPetroleum>(
                privateState.ManufacturingState.ContainerInput);

            itemsService.SetContainerType<ItemsContainerEmptyCanisters>(
                manufacturingStateProcessedGasoline.ContainerInput);

            itemsService.SetContainerType<ItemsContainerEmptyCanisters>(
                manufacturingStateProcessedMineralOil.ContainerInput);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var worldObject = data.GameObject;
            var deltaTime = data.DeltaTime;
            var privateState = data.PrivateState;
            var fuelBurningState = privateState.FuelBurningState;
            var manufacturingStateRawPetroleum = data.PrivateState.ManufacturingState;
            var manufacturingStateProcessedGasoline = data.PrivateState.ManufacturingStateGasoline;
            var manufacturingStateProcessedMineralOil = data.PrivateState.ManufacturingStateMineralOil;
            var liquidStateRawPetroleum = privateState.LiquidStateRawPetroleum;
            var liquidStateProcessedGasoline = privateState.LiquidStateGasoline;
            var liquidStateProcessedMineralOil = privateState.LiquidStateMineralOil;

            // Force update all recipes:
            // it will auto-detect and verify current recipes for every crafting queue.
            var isLiquidStatesChanged = privateState.IsLiquidStatesChanged;
            ManufacturingMechanic.UpdateRecipeOnly(
                worldObject,
                manufacturingStateRawPetroleum,
                this.ManufacturingConfig,
                force: isLiquidStatesChanged);

            ManufacturingMechanic.UpdateRecipeOnly(
                worldObject,
                manufacturingStateProcessedGasoline,
                this.ManufacturingConfigGasoline,
                force: isLiquidStatesChanged);

            ManufacturingMechanic.UpdateRecipeOnly(
                worldObject,
                manufacturingStateProcessedMineralOil,
                this.ManufacturingConfigMineralOil,
                force: isLiquidStatesChanged);

            privateState.IsLiquidStatesChanged = false;

            // Update fuel state:
            // need fuel when processed liquids capacities are not full
            // or any of the manufacturing states has active recipe.
            var isLiquidsCapacitiesFull = liquidStateProcessedGasoline.Amount
                                          >= this.LiquidConfigGasoline.Capacity
                                          && liquidStateProcessedMineralOil.Amount
                                          >= this.LiquidConfigMineralOil.Capacity;

            var isNeedFuelNow = (!isLiquidsCapacitiesFull && liquidStateRawPetroleum.Amount > 0)
                                || manufacturingStateProcessedGasoline.HasActiveRecipe
                                || manufacturingStateProcessedMineralOil.HasActiveRecipe;

            // update fuel burning progress
            FuelBurningMechanic.Update(
                worldObject,
                fuelBurningState,
                privateState.FuelBurningByproductsQueue,
                this.ManufacturingConfig,
                data.DeltaTime,
                isNeedFuelNow,
                forceRefreshFuel: isLiquidStatesChanged);

            var isFuelBurning = fuelBurningState.FuelUseTimeRemainsSeconds > 0;
            // set IsActive flag in public state - this is used to play sound and animation on client
            data.PublicState.IsManufacturingActive = isFuelBurning;

            // Update crafting queue for raw petroleum:
            // on complete it will consume petroleum canister (if available), increase oil level, produce empty canister.
            ManufacturingMechanic.UpdateCraftingQueueOnly(manufacturingStateRawPetroleum, deltaTime);

            if (!isFuelBurning)
            {
                // cannot progress while fuel is not burning
                return;
            }

            // fuel is burning, we can "transfer" liquids and progress crafting queues for processed liquids
            // try transfer ("use") raw petroleum bar
            LiquidContainerSystem.UpdateWithoutManufacturing(
                liquidStateRawPetroleum,
                this.LiquidConfigRawPetroleum,
                deltaTime,
                // petroleum is not produced via this system (it's produced on recipe completion)
                isProduceLiquid: false,
                // use petroleum liquid if other capacities are not full
                isUseRequested: !isLiquidsCapacitiesFull,
                wasUsed: out var wasUsedPetroleum,
                resetAmountToZeroWhenNotEnoughToUse: true);

            if (wasUsedPetroleum)
            {
                // increase gasoline level (if possible)
                LiquidContainerSystem.UpdateWithoutManufacturing(
                    liquidStateProcessedGasoline,
                    this.LiquidConfigGasoline,
                    deltaTime,
                    isProduceLiquid: true,
                    isUseRequested: false,
                    wasUsed: out _);

                // increase mineral oil level (if possible)
                LiquidContainerSystem.UpdateWithoutManufacturing(
                    liquidStateProcessedMineralOil,
                    this.LiquidConfigMineralOil,
                    deltaTime,
                    isProduceLiquid: true,
                    isUseRequested: false,
                    wasUsed: out _);

                // this flag is required to force recipes checking on next iteration
                privateState.IsLiquidStatesChanged = true;
            }

            // progress crafting queues for processed liquids (craft canisters with according liquids)
            ManufacturingMechanic.UpdateCraftingQueueOnly(manufacturingStateProcessedGasoline,   deltaTime);
            ManufacturingMechanic.UpdateCraftingQueueOnly(manufacturingStateProcessedMineralOil, deltaTime);
        }
    }
}