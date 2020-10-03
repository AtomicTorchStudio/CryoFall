namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    [ElectricityProductionOrder(afterType: typeof(ObjectGeneratorEngine))]
    public class ObjectGeneratorSteam
        : ProtoObjectGeneratorWithFuel
            <ObjectGeneratorSteam.PrivateState,
                ObjectGeneratorWithFuelPublicState,
                StaticObjectClientState>
    {
        public const double FuelBurningSpeedMultiplier = 4.0;

        public const double SteamTemperatureDecreasePerSecond = 2.0;

        public const ushort SteamTemperatureGenerationStart = 100;

        public const double SteamTemperatureIncreasePerSecond = 3.5;

        public const ushort SteamTemperatureMax = 200;

        public const ushort SteamTemperatureMin = 25;

        public override byte ContainerFuelSlotsCount => 4;

        public override byte ContainerInputSlotsCount => 1;

        public override byte ContainerOutputSlotsCount => 1;

        public override ElectricityThresholdsPreset DefaultGenerationElectricityThresholds
            => new ElectricityThresholdsPreset(startupPercent: 90,
                                               shutdownPercent: 100);

        public override string Description =>
            "This generator uses hot steam to drive a turbine and produce electricity. Takes time to boil water until it reaches its maximum output.";

        public override double LiquidCapacity => 100;

        public override double LiquidConsumptionAmountPerSecond => 0.025;

        public override LiquidType LiquidType => LiquidType.Water;

        public override string Name => "Steam generator";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 2500;

        public static double SharedGetElectricityProductionRate(
            IStaticWorldObject worldObject,
            out float temperature)
        {
            var privateState = GetPrivateState(worldObject);
            temperature = privateState.CurrentTemperature;

            if (privateState.LiquidState.Amount <= 0)
            {
                // cannot generate electricity when there is no water
                return 0;
            }

            var rate = (temperature - SteamTemperatureGenerationStart)
                       / (SteamTemperatureMax - SteamTemperatureGenerationStart);

            return Math.Max(rate, 0);
        }

        public override void SharedGetElectricityProduction(
            IStaticWorldObject worldObject,
            out double currentProduction,
            out double maxProduction)
        {
            maxProduction = 12;
            var rate = SharedGetElectricityProductionRate(worldObject, out _);
            currentProduction = maxProduction * rate;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var publicState = data.PublicState;
            var sceneObject = data.GameObject.ClientSceneObject;

            var soundEmitter = this.ClientCreateActiveStateSoundEmitterComponent(data.GameObject);
            soundEmitter.Volume = 0.35f;
            soundEmitter.Radius = 2f;
            soundEmitter.CustomMaxDistance = 6f;

            var componentVibration = sceneObject
                .AddComponent<ClientComponentWorldObjectVibration>();

            componentVibration.Setup(data.ClientState.Renderer,
                                     amplitude: 0.6 / 256.0,
                                     speed: 1.0,
                                     verticalStartOffsetRelative: 0.02);

            publicState.ClientSubscribe(_ => _.IsActive,
                                        _ => RefreshActiveState(),
                                        data.ClientState);

            RefreshActiveState();

            void RefreshActiveState()
            {
                componentVibration.IsEnabled = publicState.IsActive;
                soundEmitter.IsEnabled = publicState.IsActive;
            }
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            var privateState = data.PrivateState;
            return WindowGeneratorSteam.Open(
                new ViewModelWindowGeneratorSteam(
                    data.GameObject,
                    privateState,
                    this.ManufacturingConfig,
                    privateState.LiquidState,
                    this.LiquidContainerConfig));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0, 0.4);
            renderer.DrawOrderOffsetY = 0.35;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryElectricity>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemWire>(count: 3);
            build.AddStageRequiredItem<ItemIngotIron>(count: 2);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemWire>(count: 2);
            repair.AddStageRequiredItem<ItemIngotIron>(count: 1);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            // setup input container to allow only water on input
            Server.Items.SetContainerType<ItemsContainerLiquidWater>(
                data.PrivateState.ManufacturingState.ContainerInput);
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
                isUseRequested: privateState.CurrentTemperature >= SteamTemperatureGenerationStart,
                resetAmountToZeroWhenNotEnoughToUse: true);

            var isActive = privateState.LiquidState.Amount > 0;
            var fuelBurningState = privateState.FuelBurningState;
            if (fuelBurningState is not null)
            {
                // progress fuel burning
                FuelBurningMechanic.Update(
                    data.GameObject,
                    fuelBurningState,
                    privateState.FuelBurningByproductsQueue,
                    this.ManufacturingConfig,
                    data.DeltaTime * FuelBurningSpeedMultiplier,
                    byproductsQueueRate: 1,
                    isNeedFuelNow: publicState.ElectricityProducerState == ElectricityProducerState.PowerOnActive
                                   && privateState.LiquidState.Amount > 0);

                // active only when fuel is burning
                isActive = fuelBurningState.FuelUseTimeRemainsSeconds > 0;
            }

            publicState.IsActive = isActive;

            // update the temperature
            var temperature = privateState.CurrentTemperature;

            var delta = isActive
                            ? (float)(SteamTemperatureIncreasePerSecond * data.DeltaTime)
                            : -(float)(SteamTemperatureDecreasePerSecond * data.DeltaTime);

            // apply some variance in temperature gain
            delta *= RandomHelper.Range(0.8f, 1.2f);

            temperature += delta;

            privateState.CurrentTemperature = MathHelper.Clamp(temperature,
                                                               SteamTemperatureMin,
                                                               SteamTemperatureMax);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2.6, 0.825), offset: (0.2, 0.5))
                .AddShapeRectangle(size: (2.4, 1.3),   offset: (0.3, 0.6), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (2.4, 0.3),   offset: (0.3, 1.4), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (2.4, 1.3),   offset: (0.3, 0.6), group: CollisionGroups.ClickArea);
        }

        public class PrivateState : ObjectGeneratorWithFuelPrivateState
        {
            [SyncToClient(DeliveryMode.UnreliableSequenced, maxUpdatesPerSecond: 1)]
            public float CurrentTemperature { get; set; }
        }
    }
}