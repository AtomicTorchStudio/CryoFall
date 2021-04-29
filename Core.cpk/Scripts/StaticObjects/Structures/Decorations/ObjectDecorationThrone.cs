namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectDecorationThrone : ProtoObjectDecoration
    {
        public override string Description =>
            "Royal throne ideal for your main audience chamber. Let the peasants know who's the boss.";

        public override double FactionWealthScorePoints => 50;

        public override string Name => "Throne";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 1000;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.6;
        }

        protected override void PrepareDecorationConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotGold>(count: 5);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotGold>(count: 3);
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 3);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.8, 0.4), offset: (0.1, 0.2))
                .AddShapeRectangle((0.8, 0.4), offset: (0.1, 0.6), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.8, 0.3), offset: (0.1, 0.8), group: CollisionGroups.HitboxRanged);
        }
    }
}