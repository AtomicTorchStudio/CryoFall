namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectChemicalLab : ProtoObjectCraftStation
    {
        public override string Description =>
            "Allows processing of most types of complex chemicals and creation of many items based on them.";

        public override string Name => "Chemical laboratory";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 400;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 1.4;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryIndustry>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 20);
            build.AddStageRequiredItem<ItemIngotIron>(count: 2);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 2);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 10);
            repair.AddStageRequiredItem<ItemIngotIron>(count: 1);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((3, 0.75),  offset: (0, 1))
                .AddShapeRectangle((1.1, 1),   offset: (0, 0))
                .AddShapeRectangle((1.1, 1),   offset: (1.9, 0))
                .AddShapeRectangle((3, 0.75),  offset: (0, 1),     group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((1.1, 1),   offset: (0, 0),     group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((1.1, 1),   offset: (1.9, 0),   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((2.8, 0.3), offset: (0.1, 1.9), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((3, 1.25),  offset: (0, 1),     group: CollisionGroups.ClickArea)
                .AddShapeRectangle((1.1, 1),   offset: (0, 0),     group: CollisionGroups.ClickArea)
                .AddShapeRectangle((1.1, 1),   offset: (1.9, 0),   group: CollisionGroups.ClickArea);
        }
    }
}