namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectDecorationFoldingScreen : ProtoObjectDecoration
    {
        public override string Description =>
            "Folding screen with a classical depiction of a tiger, like in the old tale.";

        public override string Name => "Folding screen";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 1000;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.25;

            renderer.PositionOffset = (0.0, 0.25);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void PrepareDecorationConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 5);
            build.AddStageRequiredItem<ItemPaper>(count: 5);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 3);
            repair.AddStageRequiredItem<ItemPaper>(count: 3);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((1.9, 0.4), offset: (0.05, 0.3))
                .AddShapeRectangle((1.9, 0.4), offset: (0.05, 0.7), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((1.9, 0.4), offset: (0.05, 0.9), group: CollisionGroups.HitboxRanged);
        }
    }
}