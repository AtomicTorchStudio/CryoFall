namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    internal class ObjectDecorationStatueGold : ProtoObjectDecoration
    {
        public override string Description =>
            "True demonstration of your wealth. Only the richest of the rich can afford to build something like this.";

        public override string Name => "Golden statue";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

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
            build.AddStageRequiredItem<ItemIngotGold>(count: 5);
            build.AddStageRequiredItem<ItemCoinPenny>(count: 100);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotGold>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.8, 0.5), offset: (0.1, 0.3),  group: CollisionGroups.Default)
                .AddShapeRectangle((0.7, 1.4), offset: (0.15, 0.3), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.7, 1.4), offset: (0.15, 0.3), group: CollisionGroups.HitboxRanged);
        }
    }
}