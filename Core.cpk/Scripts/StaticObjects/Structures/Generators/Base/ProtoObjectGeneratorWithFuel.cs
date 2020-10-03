namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectGeneratorWithFuel
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectManufacturer
          <TPrivateState,
              TPublicState,
              TClientState>, IProtoObjectGeneratorWithFuel
        where TPrivateState : ObjectGeneratorWithFuelPrivateState, new()
        where TPublicState : ObjectManufacturerPublicState, IObjectElectricityProducerPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public abstract ElectricityThresholdsPreset DefaultGenerationElectricityThresholds { get; }

        public int GenerationOrder { get; set; }

        public override bool IsAutoSelectRecipe => true;

        public override bool IsFuelProduceByproducts => false;

        public override bool IsRelocatable => true;

        public abstract double LiquidCapacity { get; }

        public abstract double LiquidConsumptionAmountPerSecond { get; }

        public abstract LiquidType LiquidType { get; }

        protected LiquidContainerConfig LiquidContainerConfig { get; private set; }

        public abstract void SharedGetElectricityProduction(
            IStaticWorldObject worldObject,
            out double currentProduction,
            out double maxProduction);

        IObjectElectricityStructurePrivateState IProtoObjectElectricityProducer.GetPrivateState(IStaticWorldObject worldObject)
        {
            return GetPrivateState(worldObject);
        }

        IObjectElectricityProducerPublicState IProtoObjectElectricityProducer.GetPublicState(IStaticWorldObject worldObject)
        {
            return GetPublicState(worldObject);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            PowerGridSystem.ClientInitializeConsumerOrProducer(data.GameObject);
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            var privateState = data.PrivateState;
            return WindowGeneratorWithFuel.Open(
                new ViewModelWindowGeneratorWithFuel(
                    data.GameObject,
                    privateState,
                    this.ManufacturingConfig,
                    privateState.LiquidState,
                    this.LiquidContainerConfig));
        }

        protected override ManufacturingConfig PrepareManufacturingConfig()
        {
            this.LiquidContainerConfig = new LiquidContainerConfig(
                this.LiquidCapacity,
                amountAutoIncreasePerSecond: 0,
                amountAutoDecreasePerSecondWhenUse: this.LiquidConsumptionAmountPerSecond);

            return base.PrepareManufacturingConfig();
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var privateState = data.PrivateState;
            privateState.LiquidState ??= new LiquidContainerState();
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var privateState = data.PrivateState;
            var publicState = data.PublicState;
            var liquidContainerState = privateState.LiquidState;

            var isFull = liquidContainerState.Amount >= this.LiquidContainerConfig.Capacity;

            ManufacturingMechanic.UpdateRecipeOnly(
                data.GameObject,
                privateState.ManufacturingState,
                this.ManufacturingConfig,
                force: !isFull);

            ManufacturingMechanic.UpdateCraftingQueueOnly(
                privateState.ManufacturingState,
                deltaTime: data.DeltaTime);

            // refill liquid
            LiquidContainerSystem.UpdateWithoutManufacturing(
                liquidContainerState,
                this.LiquidContainerConfig,
                data.DeltaTime,
                isProduceLiquid: false,
                wasUsed: out _,
                isUseRequested: publicState.IsActive,
                resetAmountToZeroWhenNotEnoughToUse: true);

            var isActive = publicState.ElectricityProducerState == ElectricityProducerState.PowerOnActive
                           && privateState.LiquidState.Amount > 0;
            var fuelBurningState = privateState.FuelBurningState;
            if (fuelBurningState is not null)
            {
                // progress fuel burning
                FuelBurningMechanic.Update(
                    data.GameObject,
                    fuelBurningState,
                    privateState.FuelBurningByproductsQueue,
                    this.ManufacturingConfig,
                    data.DeltaTime,
                    byproductsQueueRate: 1,
                    isNeedFuelNow: publicState.ElectricityProducerState == ElectricityProducerState.PowerOnActive
                                   && privateState.LiquidState.Amount > 0);

                // active only when fuel is burning
                isActive = fuelBurningState.FuelUseTimeRemainsSeconds > 0;
            }

            publicState.IsActive = isActive;
        }
    }

    public abstract class ProtoObjectGeneratorWithFuel
        : ProtoObjectGeneratorWithFuel
            <ObjectGeneratorWithFuelPrivateState,
                ObjectGeneratorWithFuelPublicState,
                StaticObjectClientState>
    {
    }
}