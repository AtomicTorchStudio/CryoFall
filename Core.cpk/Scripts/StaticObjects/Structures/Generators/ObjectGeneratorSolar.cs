namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectGeneratorSolar : ProtoObjectGeneratorSolar
    {
        private const int DrawOrderOffsetY = 1;

        public override string Description =>
            "Basic installation for solar power generation. Requires individual solar panels to operate.";

        public override string Name => "Solar generator";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override byte PanelSlotsCount => 4;

        public override float StructurePointsMax => 2000;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
            => (1, 1.65);

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            data.GameObject.ClientSceneObject
                .AddComponent<ComponentGeneratorSolarPanelsRenderer>()
                .Setup(data.PublicState.PanelsContainer,
                       baseDrawOrderOffsetY: DrawOrderOffsetY);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = DrawOrderOffsetY;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
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
            build.AddStageRequiredItem<ItemWire>(count: 10);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 1);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemWire>(count: 5);
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.55, 1.325), offset: (0.225, 0.2))
                .AddShapeRectangle(size: (1.4, 1.6),    offset: (0.3, 0.2),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.4, 0.35),   offset: (0.3, 1.45), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1.2, 1.4),    offset: (0.4, 0.3),  group: CollisionGroups.ClickArea);
        }
    }
}