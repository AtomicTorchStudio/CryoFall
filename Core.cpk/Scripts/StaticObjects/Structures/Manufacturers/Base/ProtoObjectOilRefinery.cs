namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

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

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            var stateGasoline = GetPrivateState(gameObject).ManufacturingStateGasoline;
            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                stateGasoline.ContainerInput);

            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                stateGasoline.ContainerOutput);

            var stateMineralOil = GetPrivateState(gameObject).ManufacturingStateMineralOil;
            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                stateMineralOil.ContainerInput);

            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                stateMineralOil.ContainerOutput);
        }

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
            var manufacturingStateGasoline = privateState.ManufacturingStateGasoline;
            if (manufacturingStateGasoline is null)
            {
                privateState.ManufacturingStateGasoline
                    = manufacturingStateGasoline
                          = new ManufacturingState(
                              data.GameObject,
                              containerInputSlotsCount: 1,
                              containerOutputSlotsCount: 1);
            }
            else
            {
                manufacturingStateGasoline.SetSlotsCount(input: 1, output: 1);
            }

            // setup manufacturing state for mineral oil
            var manufacturingStateMineralOil = privateState.ManufacturingStateMineralOil;
            if (manufacturingStateMineralOil is null)
            {
                privateState.ManufacturingStateMineralOil
                    = manufacturingStateMineralOil
                          = new ManufacturingState(
                              data.GameObject,
                              containerInputSlotsCount: 1,
                              containerOutputSlotsCount: 1);
            }
            else
            {
                manufacturingStateMineralOil.SetSlotsCount(input: 1, output: 1);
            }

            // setup input container types
            var itemsService = Server.Items;
            itemsService.SetContainerType<ItemsContainerLiquidPetroleum>(
                privateState.ManufacturingState.ContainerInput);

            itemsService.SetContainerType<ItemsContainerEmptyCanisters>(
                manufacturingStateGasoline.ContainerInput);

            itemsService.SetContainerType<ItemsContainerEmptyCanisters>(
                manufacturingStateMineralOil.ContainerInput);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var worldObject = data.GameObject;
            var deltaTime = data.DeltaTime;
            var privateState = data.PrivateState;
            var manufacturingStateRawPetroleum = data.PrivateState.ManufacturingState;
            var manufacturingStateGasoline = data.PrivateState.ManufacturingStateGasoline;
            var manufacturingStateMineralOil = data.PrivateState.ManufacturingStateMineralOil;
            var liquidStateRawPetroleum = privateState.LiquidStateRawPetroleum;
            var liquidStateGasoline = privateState.LiquidStateGasoline;
            var liquidStateMineralOil = privateState.LiquidStateMineralOil;

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
                manufacturingStateGasoline,
                this.ManufacturingConfigGasoline,
                force: isLiquidStatesChanged);

            ManufacturingMechanic.UpdateRecipeOnly(
                worldObject,
                manufacturingStateMineralOil,
                this.ManufacturingConfigMineralOil,
                force: isLiquidStatesChanged);

            privateState.IsLiquidStatesChanged = false;

            // Update fuel state:
            // need fuel when any of the output liquids capacity are not full
            // or any of the manufacturing states has active recipe.
            var isOutputLiquidCapacityFull
                = liquidStateGasoline.Amount >= this.LiquidConfigGasoline.Capacity
                  || liquidStateMineralOil.Amount >= this.LiquidConfigMineralOil.Capacity;

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
                deltaTimeLiquidProcessing *= RateManufacturingSpeedMultiplier.SharedValue;

                // active, we can "transfer" liquids and progress crafting queues for output liquids
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
                        liquidStateGasoline,
                        this.LiquidConfigGasoline,
                        deltaTimeLiquidProcessing,
                        isProduceLiquid: true,
                        isUseRequested: false,
                        wasUsed: out _);

                    // increase mineral oil level (if possible)
                    LiquidContainerSystem.UpdateWithoutManufacturing(
                        liquidStateMineralOil,
                        this.LiquidConfigMineralOil,
                        deltaTimeLiquidProcessing,
                        isProduceLiquid: true,
                        isUseRequested: false,
                        wasUsed: out _);

                    // this flag is required to force recipes checking on next iteration
                    privateState.IsLiquidStatesChanged = true;
                }
            }

            // progress crafting queues for output liquids (craft canisters with according liquids)
            ManufacturingMechanic.UpdateCraftingQueueOnly(manufacturingStateGasoline,   deltaTime);
            ManufacturingMechanic.UpdateCraftingQueueOnly(manufacturingStateMineralOil, deltaTime);
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