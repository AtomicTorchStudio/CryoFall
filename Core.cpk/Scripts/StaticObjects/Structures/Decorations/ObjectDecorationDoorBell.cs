namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectDecorationDoorBell : ProtoObjectDecorationWithSoundSource
    {
        public override string Description =>
            "Expecting unannounced visitors with guns? Better put this doorbell outside.";

        public override string Name => "Doorbell";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 750;

        protected override ReadOnlySoundResourceSet SoundSet
            => new SoundResourceSet()
               .Add("Objects/Structures/Object" + this.ShortId + "/Action")
               .ToReadOnly();

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
            build.AddStageRequiredItem<ItemPlanks>(count: 5);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 5);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.6, 0.4),  offset: (0.2, 0.2))
                .AddShapeRectangle((0.6, 0.4),  offset: (0.2, 0.9),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.6, 0.3),  offset: (0.2, 1.1),  group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((0.6, 1.25), offset: (0.2, 0.28), group: CollisionGroups.ClickArea);
        }
    }
}