namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Water pump is a manufacturer: empty bottle -> bottle with water.
    /// </summary>
    public class ObjectWaterPump : ProtoObjectWell
    {
        public override string Description =>
            "Uses electrical pump to quickly extract water from underground reservoir.";

        public override double ElectricityConsumptionPerSecondWhenActive => 2;

        public override string Name => "Water pump";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 2000;

        public override double WaterCapacity => 100;

        public override double WaterProductionAmountPerSecond => 2.0;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var publicState = data.PublicState;
            var sceneObject = data.GameObject.ClientSceneObject;

            var soundEmitter =
                this.ClientCreateActiveStateSoundEmitterComponent(data.GameObject);
            soundEmitter.Radius = 1f;
            soundEmitter.CustomMaxDistance = 5f;
            soundEmitter.Volume = 0.5f;

            publicState.ClientSubscribe(_ => _.IsActive,
                                        _ => RefreshActiveState(),
                                        data.ClientState);

            RefreshActiveState();

            void RefreshActiveState()
            {
                soundEmitter.IsEnabled = publicState.IsActive;
            }
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);

            renderer.SpritePivotPoint = (0.5, 0);
            renderer.PositionOffset = (1, 0.3);
            renderer.DrawOrderOffsetY = 0.6;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override void PrepareConstructionConfigWell(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryIndustry>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemWire>(count: 5);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 1);
            build.AddStageRequiredItem<ItemRubberVulcanized>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemWire>(count: 2);
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Default);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.5, 1.1), offset: (0.25, 0.4))
                .AddShapeRectangle(size: (1.2, 1.3), offset: (0.4, 0.5),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.2, 0.3), offset: (0.4, 1.25), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1.2, 1.3), offset: (0.4, 0.5),  group: CollisionGroups.ClickArea);
        }
    }
}