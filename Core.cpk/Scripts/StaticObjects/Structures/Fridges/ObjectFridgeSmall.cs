namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectFridgeSmall : ProtoObjectFridgeElectrical
    {
        public override string Description =>
            "Convenient way to store perishable food. Fridge will keep it fresh much longer.";

        public override double ElectricityConsumptionPerSecondWhenActive => 1;

        public override double FreshnessDurationMultiplier => 10;

        public override bool HasOwnersList => false;

        public override byte ItemsSlotsCount => 16;

        public override string Name => "Small fridge";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 250;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.3;
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryStorage>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            build.AddStageRequiredItem<ItemWire>(count: 4);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
            repair.AddStageRequiredItem<ItemWire>(count: 2);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.9, 0.6),
                                   offset: (0.05, 0))
                .AddShapeRectangle(size: (1, 1),
                                   offset: (0, 0),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 0.25),
                                   offset: (0.1, 0.85),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1, 1.45),
                                   offset: (0, 0),
                                   group: CollisionGroups.ClickArea);
        }
    }
}