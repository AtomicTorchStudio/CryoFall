namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.State;

    public abstract class ProtoObjectOilRefinery
        : ProtoObjectManufacturer<
            ProtoObjectOilRefinery.PrivateState,
            ObjectManufacturerPublicState,
            StaticObjectClientState>
    {
        public sealed override byte ContainerFuelSlotsCount => 0;

        public override bool IsAutoSelectRecipe => true;

        public override bool IsFuelProduceByproducts => false;

        /// <summary>
        /// Capacity of gasoline in refinery.
        /// </summary>
        public abstract double LiquidCapacityGasoline { get; }

        /// <summary>
        /// Capacity of mineral oil in refinery.
        /// </summary>
        public abstract double LiquidCapacityMineralOil { get; }

        /// <summary>
        /// Capacity of raw petroleum in refinery.
        /// </summary>
        public abstract double LiquidCapacityRawPetroleum { get; }

        /// <summary>
        /// Gasoline production amount per second.
        /// </summary>
        public abstract double LiquidGasolineProductionPerSecond { get; }

        /// <summary>
        /// Mineral oil production amount per second.
        /// </summary>
        public abstract double LiquidMineralOilProductionPerSecond { get; }

        /// <summary>
        /// Raw petroleum consumption amount per second.
        /// </summary>
        public abstract double LiquidRawPetroleumConsumptionPerSecond { get; }

        protected LiquidContainerConfig LiquidConfigGasoline { get; private set; }

        protected LiquidContainerConfig LiquidConfigMineralOil { get; private set; }

        protected LiquidContainerConfig LiquidConfigRawPetroleum { get; private set; }

        protected ManufacturingConfig ManufacturingConfigGasoline { get; private set; }

        protected ManufacturingConfig ManufacturingConfigMineralOil { get; private set; }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            var privateState = data.PrivateState;
            return WindowOilRefinery.Open(
                new ViewModelWindowOilRefinery(
                    data.GameObject,
                    privateState.ManufacturingState,
                    privateState.ManufacturingStateGasoline,
                    privateState.ManufacturingStateMineralOil,
                    this.ManufacturingConfig,
                    this.ManufacturingConfigGasoline,
                    this.ManufacturingConfigMineralOil,
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
                recipes: new[] { GetProtoEntity<RecipeOilRefineryEmptyCanisterFromPetroleumCanister>() },
                recipesForByproducts,
                isProduceByproducts: this.IsFuelProduceByproducts,
                isAutoSelectRecipe: this.IsAutoSelectRecipe);
        }

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.ManufacturingConfigGasoline = new ManufacturingConfig(
                this,
                recipes: new[] { GetProtoEntity<RecipeOilRefineryGasolineCanister>() },
                recipesForByproducts: null,
                isProduceByproducts: false,
                isAutoSelectRecipe: true);

            this.ManufacturingConfigMineralOil = new ManufacturingConfig(
                this,
                recipes: new[] { GetProtoEntity<RecipeOilRefineryMineralOilCanister>() },
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
            privateState.LiquidStateRawPetroleum ??= new LiquidContainerState();
            privateState.LiquidStateGasoline ??= new LiquidContainerState();
            privateState.LiquidStateMineralOil ??= new LiquidContainerState();

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
            // need fuel when any of the output liquids capacity are not full
            // or any of the manufacturing states has active recipe.
            var isOutputLiquidCapacityFull
                = liquidStateProcessedGasoline.Amount >= this.LiquidConfigGasoline.Capacity
                  || liquidStateProcessedMineralOil.Amount >= this.LiquidConfigMineralOil.Capacity;

            var isNeedElectricityNow = !isOutputLiquidCapacityFull
                                       && liquidStateRawPetroleum.Amount > 0;

            // Consuming electricity.
            // Active only if electricity state is on and has active recipe.
            var isActive = false;
            var publicState = data.PublicState;
            if (publicState.ElectricityConsumerState == ElectricityConsumerState.PowerOnActive)
            {
                isActive = isNeedElectricityNow;
            }

            // set IsActive flag in public state - this is used to play sound and animation on client
            data.PublicState.IsActive = isActive;

            // Update crafting queue for raw petroleum:
            // on complete it will consume petroleum canister (if available), increase oil level, produce empty canister.
            ManufacturingMechanic.UpdateCraftingQueueOnly(manufacturingStateRawPetroleum, deltaTime);

            if (isActive) // process liquids (consume raw petroleum and produce gasoline and mineral oil)
            {
                // apply extraction rate multiplier (it applies to oil refinery production rate)
                var deltaTimeLiquidProcessing = deltaTime;
                deltaTimeLiquidProcessing *= StructureConstants.ManufacturingSpeedMultiplier;

                // active, we can "transfer" liquids and progress crafting queues for processed liquids
                // try transfer ("use") raw petroleum bar
                LiquidContainerSystem.UpdateWithoutManufacturing(
                    liquidStateRawPetroleum,
                    this.LiquidConfigRawPetroleum,
                    deltaTimeLiquidProcessing,
                    // petroleum is not produced via this system (it's produced on recipe completion)
                    isProduceLiquid: false,
                    // use petroleum liquid if other capacities are not full
                    isUseRequested: !isOutputLiquidCapacityFull,
                    wasUsed: out var wasUsedPetroleum,
                    resetAmountToZeroWhenNotEnoughToUse: true);

                if (wasUsedPetroleum)
                {
                    // increase gasoline level (if possible)
                    LiquidContainerSystem.UpdateWithoutManufacturing(
                        liquidStateProcessedGasoline,
                        this.LiquidConfigGasoline,
                        deltaTimeLiquidProcessing,
                        isProduceLiquid: true,
                        isUseRequested: false,
                        wasUsed: out _);

                    // increase mineral oil level (if possible)
                    LiquidContainerSystem.UpdateWithoutManufacturing(
                        liquidStateProcessedMineralOil,
                        this.LiquidConfigMineralOil,
                        deltaTimeLiquidProcessing,
                        isProduceLiquid: true,
                        isUseRequested: false,
                        wasUsed: out _);

                    // this flag is required to force recipes checking on next iteration
                    privateState.IsLiquidStatesChanged = true;
                }
            }

            // progress crafting queues for processed liquids (craft canisters with according liquids)
            ManufacturingMechanic.UpdateCraftingQueueOnly(manufacturingStateProcessedGasoline,   deltaTime);
            ManufacturingMechanic.UpdateCraftingQueueOnly(manufacturingStateProcessedMineralOil, deltaTime);
        }

        public class PrivateState : ObjectManufacturerPrivateState
        {
            [TempOnly]
            public bool IsLiquidStatesChanged { get; set; }

            [SyncToClient]
            public LiquidContainerState LiquidStateGasoline { get; set; }

            [SyncToClient]
            public LiquidContainerState LiquidStateMineralOil { get; set; }

            [SyncToClient]
            public LiquidContainerState LiquidStateRawPetroleum { get; set; }

            [SyncToClient]
            public ManufacturingState ManufacturingStateGasoline { get; set; }

            [SyncToClient]
            public ManufacturingState ManufacturingStateMineralOil { get; set; }
        }
    }
}