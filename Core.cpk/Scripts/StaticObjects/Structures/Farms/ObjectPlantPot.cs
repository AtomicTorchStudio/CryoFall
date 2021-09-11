namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectPlantPot : ProtoObjectPlantPot
    {
        public override string Description =>
            "Can be used to grow flowers and decorate your house or collect flowers for other purposes. Can be constructed anywhere.";

        public override bool IsDrawingPlantShadow => false;

        public override string Name => "Plant pot";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override Vector2D PlacedPlantPositionOffset { get; } = (0, 0.25);

        public override float StructurePointsMax => 400;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (renderer.PositionOffset.X, 0.2);
            renderer.DrawOrderOffsetY = 0.33;
            renderer.Scale = 0.75;
        }

        protected override void PrepareFarmConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 1;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemClay>(count: 5);
            build.AddStageRequiredItem<ItemSand>(count: 5);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemClay>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.23,
                    center: (0.5, 0.46))
                .AddShapeRectangle(
                    size: (0.8, 0.7),
                    offset: (0.1, 0.1),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (0.9, 0.8),
                    offset: (0.05, 0.05),
                    group: CollisionGroups.HitboxRanged);
        }
    }
}