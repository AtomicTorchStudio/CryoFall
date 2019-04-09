namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectFarmingWorkbench : ProtoObjectCraftStation
    {
        public override string Description =>
            "Used to produce various items related to farming, such as seed bags and fertilizers.";

        public override string Name => "Farming workbench";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 300;

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
            category = GetCategory<StructureCategoryFood>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 10);
            build.AddStageRequiredItem<ItemStone>(count: 5);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 5);
            repair.AddStageRequiredItem<ItemStone>(count: 2);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((2, 1))
                .AddShapeRectangle((2, 1), offset: (0, 0), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    (1.8, 1),
                    offset: (0.1, 0),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((2, 1), offset: (0, 0), group: CollisionGroups.ClickArea);
        }
    }
}