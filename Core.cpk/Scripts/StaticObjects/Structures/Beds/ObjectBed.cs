namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Beds
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectBed : ProtoObjectBed
    {
        public override string Description =>
            "Comfortable improvement over a bedroll.";

        public override string Name => "Bed";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override double RespawnCooldownDurationSeconds => 2 * 60;

        public override float StructurePointsMax => 1200;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 0.4;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryOther>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 5);
            build.AddStageRequiredItem<ItemFibers>(count: 10);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 3);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.9, 0.575),  offset: (0.05, 0.125))
                .AddShapeRectangle(size: (1.8, 0.9),  offset: (0.1, 0.1),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.8, 0.15), offset: (0.1, 0.85), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1.8, 0.9),  offset: (0.1, 0.1),  group: CollisionGroups.ClickArea);
        }
    }
}