namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    [ElectricityProductionOrder(afterType: typeof(ObjectGeneratorBio))]
    public class ObjectGeneratorEngine : ProtoObjectGeneratorWithFuel
    {
        public override byte ContainerFuelSlotsCount => 0;

        public override byte ContainerInputSlotsCount => 2;

        public override byte ContainerOutputSlotsCount => 2;

        public override ElectricityThresholdsPreset DefaultGenerationElectricityThresholds
            => new(startupPercent: 90,
                   shutdownPercent: 100);

        public override string Description =>
            "This large generator uses gasoline fuel to produce electrical energy.";

        public override double LiquidCapacity => 50;

        public override double LiquidConsumptionAmountPerSecond => 0.2;

        public override LiquidType LiquidType => LiquidType.Gasoline;

        public override string Name => "Engine generator";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 5000;

        public override void SharedGetElectricityProduction(
            IStaticWorldObject worldObject,
            out double currentProduction,
            out double maxProduction)
        {
            var publicState = GetPublicState(worldObject);

            maxProduction = 15;
            currentProduction = publicState.IsActive
                                    ? maxProduction
                                    : 0;
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
                                     amplitude: 0.8 / 256.0,
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
            tileRequirements.Add(LandClaimSystem.ValidatorIsOwnedLand);
            category = GetCategory<StructureCategoryElectricity>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemWire>(count: 5);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 3);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemWire>(count: 5);
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 2);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            // setup input container to allow only petroleum on input
            Server.Items.SetContainerType<ItemsContainerLiquidGasoline>(
                data.PrivateState.ManufacturingState.ContainerInput);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2.8, 0.825), offset: (0.1, 0.5))
                .AddShapeRectangle(size: (2.6, 1.4),   offset: (0.2, 0.5), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (2.6, 0.3),   offset: (0.2, 1.4), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (2.6, 1.4),   offset: (0.2, 0.5), group: CollisionGroups.ClickArea);
        }
    }
}