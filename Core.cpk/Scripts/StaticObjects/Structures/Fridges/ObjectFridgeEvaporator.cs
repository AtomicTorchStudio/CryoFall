namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectFridgeEvaporator : ProtoObjectFridge
    {
        public override string Description =>
            "Primitive cooler that uses evaporation as its primary cooling method. Isn't as effective as modern fridges, but will still keep your food fresh for longer.";

        public override double FreshnessDurationMultiplier => 5;

        public override bool HasOwnersList => false;

        public override bool IsSupportItemIcon => false;

        public override byte ItemsSlotsCount => 8;

        public override string Name => "Primitive fridge";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 500;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 0.25;
            renderer.PositionOffset += (0, 0.25);
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryStorage>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 5);
            build.AddStageRequiredItem<ItemBottleWater>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var yOffset = 0.25;
            data.PhysicsBody
                .AddShapeRectangle((0.7, 0.45), offset: (0.15, yOffset + 0.05))
                .AddShapeRectangle((0.8, 1.1),  offset: (0.1, yOffset), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.7, 0.2),  offset: (0.15, 0.85),   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((0.8, 1.1),  offset: (0.1, yOffset), group: CollisionGroups.ClickArea);
        }
    }
}