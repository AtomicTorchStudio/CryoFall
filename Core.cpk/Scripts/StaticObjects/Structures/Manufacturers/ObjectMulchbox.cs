namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectMulchbox : ProtoObjectMulchbox
    {
        public override string Description =>
            "Can be used to convert any kind of organic matter into mulch. Essentially an organic fertilizer.";

        public override string Name => "Mulchbox";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 1;

        public override ushort OrganicCapacity => 250;

        public override float StructurePointsMax => 250;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 1;
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
            category = GetCategory<StructureCategoryFood>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 15);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 5);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(
                    size: (1.8, 1.1),
                    offset: (0.1, 0))
                .AddShapeRectangle(
                    size: (1.6, 1.7),
                    offset: (0.2, 0.1),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (1.6, 1.7),
                    offset: (0.2, 0.1),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(
                    size: (1.6, 1.7),
                    offset: (0.2, 0.1),
                    group: CollisionGroups.ClickArea);
        }
    }
}