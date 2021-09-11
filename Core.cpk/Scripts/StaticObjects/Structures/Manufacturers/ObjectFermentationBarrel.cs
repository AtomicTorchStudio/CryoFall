namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectFermentationBarrel : ProtoObjectManufacturer
    {
        public override byte ContainerFuelSlotsCount => 0;

        public override byte ContainerInputSlotsCount => 4;

        public override byte ContainerOutputSlotsCount => 4;

        public override string Description =>
            "Fermentation barrel allows production of different alcoholic beverages and concoctions.";

        public override bool IsAutoSelectRecipe => false;

        public override bool IsFuelProduceByproducts => false;

        public override string Name => "Fermentation barrel";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 1.0;

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
            category = GetCategory<StructureCategoryFood>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 5);
            build.AddStageRequiredItem<ItemIngotIron>(count: 1);
            build.AddStageRequiredItem<ItemRope>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 5);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.9, 0.5),
                                   offset: (0.05, 0.1))
                .AddShapeRectangle(size: (0.8, 0.9),
                                   offset: (0.1, 0.2),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 0.2),
                                   offset: (0.1, 0.95),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.9, 0.9),
                                   offset: (0.05, 0.1),
                                   group: CollisionGroups.ClickArea);
        }
    }
}