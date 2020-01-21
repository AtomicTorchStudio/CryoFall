namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectFridgeLarge : ProtoObjectFridgeElectrical
    {
        public override string Description =>
            "Convenient way to store large amounts of perishable food. Fridge will keep it fresh much longer.";

        public override double ElectricityConsumptionPerSecondWhenActive => 0.25;

        public override double FreshnessDurationMultiplier => 10;

        public override bool HasOwnersList => false;

        public override byte ItemsSlotsCount => 24;

        public override string Name => "Large fridge";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 3000;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.2;
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
            build.AddStageRequiredItem<ItemIngotSteel>(count: 3);
            build.AddStageRequiredItem<ItemWire>(count: 3);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
            repair.AddStageRequiredItem<ItemWire>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 0.75),
                                   offset: (0, 0))
                .AddShapeRectangle(size: (1, 1.35),
                                   offset: (0, 0),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.9, 0.3),
                                   offset: (0.05, 0.85),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1, 1.95),
                                   offset: (0, 0),
                                   group: CollisionGroups.ClickArea);
        }
    }
}