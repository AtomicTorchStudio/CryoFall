namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPowerStorage : ProtoObjectPowerStorage
    {
        public override string Description =>
            "Allows one to store electrical energy produced by generators on the base for later use.";

        public override double ElectricityCapacity => 10000;

        public override string Name => "Power storage";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 2000;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryElectricity>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemWire>(count: 10);
            build.AddStageRequiredItem<ItemIngotIron>(count: 1);
            build.AddStageRequiredItem<ItemAcidSulfuric>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemWire>(count: 5);
            repair.AddStageRequiredItem<ItemIngotIron>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.9, 0.6),  offset: (0.05, 0))
                .AddShapeRectangle(size: (0.8, 0.4),  offset: (0.1, 0.6),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 0.2),  offset: (0.1, 0.75), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.9, 1.17), offset: (0.05, 0),   group: CollisionGroups.ClickArea);
        }
    }
}