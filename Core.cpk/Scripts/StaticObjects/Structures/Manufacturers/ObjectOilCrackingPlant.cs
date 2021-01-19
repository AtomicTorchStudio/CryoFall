namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectOilCrackingPlant : ProtoObjectOilCrackingPlant
    {
        public override byte ContainerInputSlotsCount => 1;

        public override byte ContainerOutputSlotsCount => 1;

        public override ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new(startupPercent: 30,
                   shutdownPercent: 20);

        public override string Description =>
            "This plant enables chemical cracking of mineral oil into lighter hydrocarbons yielding gasoline specifically.";

        public override double ElectricityConsumptionPerSecondWhenActive => 5;

        public override double LiquidCapacityGasoline => 50;

        public override double LiquidCapacityMineralOil => 50;

        // A little larger number is used for liquids production speed
        // to fix an issue with the floating point precision
        // (e.g. a case when 0.0001 liquid level shortage prevents from collecting a full canister).
        // With the adjusted number we have a barely noticeable profit which resolves the issue.
        public override double LiquidGasolineProductionPerSecond => 0.2002;

        public override double LiquidMineralOilConsumptionPerSecond => 0.2;

        public override string Name => "Oil cracking plant";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 10000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var worldObject = data.GameObject;
            var publicState = data.PublicState;

            var soundEmitter = this.ClientCreateActiveStateSoundEmitterComponent(data.GameObject);
            soundEmitter.Volume = 0.35f;
            soundEmitter.Radius = 1f;
            soundEmitter.CustomMaxDistance = 4f;

            var sceneObject = worldObject.ClientSceneObject;
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
            renderer.DrawOrderOffsetY = 0.4;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryIndustry>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            build.AddStageRequiredItem<ItemIngotGold>(count: 1);
            build.AddStageRequiredItem<ItemCement>(count: 2);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(
            CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2.0, 1.0),  offset: (0, 0))
                .AddShapeRectangle(size: (1.8, 0.6),  offset: (0.1, 0.5),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.8, 0.4),  offset: (0.1, 1.1),  group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1.9, 1.17), offset: (0.05, 0.4), group: CollisionGroups.ClickArea);
        }
    }
}