namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;

    public class ObjectRechargingStation : ProtoObjectRechargingStation
    {
        public override byte ContainerInputSlotsCount => 4;

        public override string Description =>
            "Can be used to recharge any portable electronic equipment such as a powerbank or flashlight.";

        public override double ElectricityChargePerSecondWhenActive =>
            16; // when there are 4 slots it means that each slow will get 16/4 = 4 EU/s

        public override string Name => "Recharging station";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 2000;

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
            build.AddStageRequiredItem<ItemWire>(count: 5);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 1);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemWire>(count: 2);
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
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