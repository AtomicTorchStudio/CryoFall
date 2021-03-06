﻿namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectDecorationBookshelf : ProtoObjectDecoration
    {
        public override string Description => "What room is complete without a bookshelf?";

        public override string Name => "Bookshelf";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 1000;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (0.5, 0.2);
            renderer.DrawOrderOffsetY = 0.25;
        }

        protected override void PrepareDecorationConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 10);
            build.AddStageRequiredItem<ItemPaper>(count: 5);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 10);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.9, 0.5), offset: (0.05, 0.3))
                .AddShapeRectangle((0.9, 0.6), offset: (0.05, 0.7), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.9, 0.35), offset: (0.05, 1.0), group: CollisionGroups.HitboxRanged);
        }
    }
}