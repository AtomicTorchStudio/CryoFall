namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.CraftRecipes.OilCrackingPlant;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectOilCrackingPlant
        : ProtoObjectManufacturer<
            ProtoObjectOilCrackingPlant.PrivateState,
            ObjectManufacturerPublicState,
            StaticObjectClientState>
    {
        public sealed override byte ContainerFuelSlotsCount => 0;

        public override bool IsAutoSelectRecipe => true;

        public override bool IsFuelProduceByproducts => false;

        /// <summary>
        /// Capacity of gasoline in mineral oil processor.
        /// </summary>
        public abstract double LiquidCapacityGasoline { get; }

        /// <summary>
        /// Capacity of mineral oil in mineral oil processor.
        /// </summary>
        public abstract double LiquidCapacityMineralOil { get; }

        /// <summary>
        /// Gasoline production amount per second.
        /// </summary>
        public abstract double LiquidGasolineProductionPerSecond { get; }

        /// <summary>
        /// Mineral oil consumption amount per second.
        /// </summary>
        public abstract double LiquidMineralOilConsumptionPerSecond { get; }

        protected LiquidContainerConfig LiquidConfigGasoline { get; private set; }

        protected LiquidContainerConfig LiquidConfigMineralOil { get; private set; }

        protected ManufacturingConfig ManufacturingConfigGasoline { get; private set; }

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
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            var privateState = data.PrivateState;
            return WindowOilCrackingPlant.Open(
                new ViewModelWindowOilCrackingPlant(
                    data.GameObject,
                    privateState.ManufacturingState,
                    privateState.ManufacturingStateGasoline,
                    this.ManufacturingConfig,
                    this.ManufacturingConfigGasoline,
                    privateState.LiquidStateMineralOil,
                    privateState.LiquidStateGasoline,
                    this.LiquidConfigMineralOil,
                    this.LiquidConfigGasoline));
        }

        protected override ManufacturingConfig PrepareManufacturingConfig()
        {
            var recipesForByproducts = FindProtoEntities<Recipe.RecipeForManufacturingByproduct>()
                .Where(r => r.StationTypes.Count == 0 || r.StationTypes.Contains(this));

            return new ManufacturingConfig(
                this,
                recipes: new[] { GetProtoEntity<RecipeOilCrackingPlantEmptyCanisterFromMineralOilCanister>() },
                recipesForByproducts,
                isProduceByproducts: this.IsFuelProduceByproducts,
                isAutoSelectRecipe: this.IsAutoSelectRecipe);
        }

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.ManufacturingConfigGasoline = new ManufacturingConfig(
                this,
                recipes: new[] { GetProtoEntity<RecipeOilCrackingPlantGasolineCanister>() },
                recipesForByproducts: null,
                isProduceByproducts: false,
                isAutoSelectRecipe: true);

            this.LiquidConfigMineralOil = new LiquidContainerConfig(
                capacity: this.LiquidCapacityMineralOil,
                // Not increased via "generation"
                // it's increased when a special recipe (mineral oil canister->empty canister) is completed.
                amountAutoIncreasePerSecond: 0,
                amountAutoDecreasePerSecondWhenUse: this.LiquidMineralOilConsumptionPerSecond);

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

            // setup input container types
            var itemsService = Server.Items;
            itemsService.SetContainerType<ItemsContainerLiquidMineralOil>(
                privateState.ManufacturingState.ContainerInput);

            itemsService.SetContainerType<ItemsContainerEmptyCanisters>(
                manufacturingStateProcessedGasoline.ContainerInput);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var worldObject = data.GameObject;
            var deltaTime = data.DeltaTime;
            var privateState = data.PrivateState;
            var manufacturingStateMineralOil = data.PrivateState.ManufacturingState;
            var manufacturingStateProcessedGasoline = data.PrivateState.ManufacturingStateGasoline;
            var liquidStateRawMineralOil = privateState.LiquidStateMineralOil;
            var liquidStateProcessedGasoline = privateState.LiquidStateGasoline;

            // Force update all recipes:
            // it will auto-detect and verify current recipes for every crafting queue.
            var isLiquidStatesChanged = privateState.IsLiquidStatesChanged;
            ManufacturingMechanic.UpdateRecipeOnly(
                worldObject,
                manufacturingStateMineralOil,
                this.ManufacturingConfig,
                force: isLiquidStatesChanged);

            ManufacturingMechanic.UpdateRecipeOnly(
                worldObject,
                manufacturingStateProcessedGasoline,
                this.ManufacturingConfigGasoline,
                force: isLiquidStatesChanged);

            privateState.IsLiquidStatesChanged = false;

            // Update fuel state:
            // need fuel when any of the output liquids capacity are not full
            // or any of the manufacturing states has active recipe.
            var isOutputLiquidCapacityFull
                = liquidStateProcessedGasoline.Amount >= this.LiquidConfigGasoline.Capacity;

            var isNeedElectricityNow = !isOutputLiquidCapacityFull
                                       && liquidStateRawMineralOil.Amount > 0;

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

            // Update crafting queue for mineral oil:
            // on complete it will consume mineral oil canister (if available), increase oil level, produce empty canister.
            ManufacturingMechanic.UpdateCraftingQueueOnly(manufacturingStateMineralOil, deltaTime);

            if (isActive) // process liquids (consume mineral oil and produce gasoline)
            {
                // apply extraction rate multiplier (it applies to mineral oil processor production rate)
                var deltaTimeLiquidProcessing = deltaTime;
                deltaTimeLiquidProcessing *= StructureConstants.ManufacturingSpeedMultiplier;

                // active, we can "transfer" liquids and progress crafting queues for processed liquids
                // try transfer ("use") mineral oil bar
                LiquidContainerSystem.UpdateWithoutManufacturing(
                    liquidStateRawMineralOil,
                    this.LiquidConfigMineralOil,
                    deltaTimeLiquidProcessing,
                    // mineral oil is not produced via this system (it's produced on recipe completion)
                    isProduceLiquid: false,
                    // use mineral oil if output capacity are not full
                    isUseRequested: !isOutputLiquidCapacityFull,
                    wasUsed: out var wasUsedMineralOil,
                    resetAmountToZeroWhenNotEnoughToUse: true);

                if (wasUsedMineralOil)
                {
                    // increase gasoline level (if possible)
                    LiquidContainerSystem.UpdateWithoutManufacturing(
                        liquidStateProcessedGasoline,
                        this.LiquidConfigGasoline,
                        deltaTimeLiquidProcessing,
                        isProduceLiquid: true,
                        isUseRequested: false,
                        wasUsed: out _);

                    // this flag is required to force recipes checking on next iteration
                    privateState.IsLiquidStatesChanged = true;
                }
            }

            // progress crafting queues for processed liquids (craft canisters with according liquids)
            ManufacturingMechanic.UpdateCraftingQueueOnly(manufacturingStateProcessedGasoline, deltaTime);
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
            public ManufacturingState ManufacturingStateGasoline { get; set; }
        }
    }
}