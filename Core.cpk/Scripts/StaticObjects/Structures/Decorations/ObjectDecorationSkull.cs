namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    internal class ObjectDecorationSkull : ProtoObjectDecoration
    {
        public override string Description =>
            "Show your enemies that you aren't joking around. No trespassing will be tolerated!";

        public override string Name => "Skull on a spike";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 200;

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
            build.AddStageRequiredItem<ItemTwigs>(count: 5);
            build.AddStageRequiredItem<ItemThread>(count: 3);
            build.AddStageRequiredItem<ItemBones>(count: 2);
            build.AddStageRequiredItem<ItemStone>(count: 2);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemTwigs>(count: 4);
            repair.AddStageRequiredItem<ItemThread>(count: 2);
            repair.AddStageRequiredItem<ItemBones>(count: 1);
            repair.AddStageRequiredItem<ItemStone>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.5, 0.4), offset: (0.25, 0.3), group: CollisionGroups.Default)
                .AddShapeRectangle((0.5, 1.4), offset: (0.25, 0.3), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.5, 1.4), offset: (0.25, 0.3), group: CollisionGroups.HitboxRanged);
        }
    }
}