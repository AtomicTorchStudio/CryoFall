namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectArmorerWorkbench : ProtoObjectCraftStation
    {
        public override string Description => "Allows creation of armor and apparel in general.";

        public override string Name => "Armor workbench";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 300;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return base.SharedGetObjectCenterWorldOffset(worldObject)
                   + (0, 0.5);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 1.3;
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
            build.AddStageRequiredItem<ItemPlanks>(count: 12);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 10);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((3, 0.8), offset: (0, 1.2))
                .AddShapeRectangle((1, 0.5), offset: (0, 0.7))
                .AddShapeRectangle((3, 1),
                                   offset: (0, 1.2),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    (1, 0.5),
                    offset: (0, 0.7),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    (3, 1),
                    offset: (0, 1.2),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(
                    (1, 0.5),
                    offset: (0, 0.7),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((3, 1.3), offset: (0, 1.2), group: CollisionGroups.ClickArea)
                .AddShapeRectangle(
                    (1, 0.5),
                    offset: (0, 0.7),
                    group: CollisionGroups.ClickArea);
        }
    }
}