namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Water collector is a manufacturer: empty bottle -> bottle with water.
    /// </summary>
    public class ObjectWaterCollector : ProtoObjectWell
    {
        public override string Description =>
            "Simple water collector designed to gather small amounts of water. Doesn't really work in deserts and other rocky places, for some reason...";

        public override string Name => "Water collector";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 300;

        public override float WaterCapacity => 25;

        public override float WaterProductionAmountPerSecond => 0.05f;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);

            renderer.DrawOrderOffsetY = 0.45;

            renderer.SpritePivotPoint = (0.5, 0);
            renderer.PositionOffset = (1, 0);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void PrepareConstructionConfigWell(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryIndustry>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemStone>(count: 3);
            build.AddStageRequiredItem<ItemTwigs>(count: 3);
            build.AddStageRequiredItem<ItemFibers>(count: 3);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemStone>(count: 2);
            repair.AddStageRequiredItem<ItemTwigs>(count: 2);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Default);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.4, center: (0.75, 0.5))
                .AddShapeCircle(radius: 0.4, center: (1.25, 0.5))
                .AddShapeRectangle((1.4, 0.7), (0.3, 0.45),  CollisionGroups.HitboxMelee)
                .AddShapeRectangle((1.3, 0.7), (0.35, 0.35), CollisionGroups.HitboxRanged)
                .AddShapeRectangle((1.4, 0.8), (0.3, 0.25),  CollisionGroups.ClickArea);
        }
    }
}