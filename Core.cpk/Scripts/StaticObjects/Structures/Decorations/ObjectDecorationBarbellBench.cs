namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectDecorationBarbellBench : ProtoObjectDecoration
    {
        public override string Description => "Most people just end up using it as a decoration...";

        public override string Name => "Barbell bench";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 1000;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.8;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override void PrepareDecorationConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotIron>(count: 2);
            build.AddStageRequiredItem<ItemLeather>(count: 2);
            build.AddStageRequiredItem<ItemFibers>(count: 5);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotIron>(count: 1);
            repair.AddStageRequiredItem<ItemLeather>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((1.5, 0.7), offset: (0.5, 0.0))
                .AddShapeRectangle((1.8, 0.5), offset: (0.1, 0.7))
                .AddShapeRectangle((0.6, 0.35), offset: (0.95, 1.2))

                .AddShapeRectangle((1.0, 1.2), offset: (0.5, 0.3), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((1.8, 0.5), offset: (0.1, 1.2), group: CollisionGroups.HitboxRanged);
        }
    }
}