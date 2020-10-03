namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectWaterCondenser : ProtoObjectWell
    {
        public const string ErrorBuildingAllowedOnlyInDesert
            = "You can build this structure only in the desert environment.";

        public override string Description =>
            "This structure can extract water directly from the ambient air by means of condensation and use of special desiccants. Unlike conventional means of obtaining water, the process is slow and inefficient, but works even in the desert.";

        public override double ElectricityConsumptionPerSecondWhenActive => 4;

        public override string Name => "Atmospheric water condenser";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 4000;

        public override double WaterCapacity => 100;

        public override double WaterProductionAmountPerSecond => 0.1;

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
            tileRequirements.Add(ErrorBuildingAllowedOnlyInDesert,
                                 c => c.Tile.ProtoTile is TileBarren);

            this.PrepareConstructionConfigWell(tileRequirements, build, repair, upgrade, out category);
        }

        protected override void PrepareConstructionConfigWell(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryIndustry>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            build.AddStageRequiredItem<ItemHygroscopicGranules>(count: 10);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.4, center: (0.5, 0.45))
                .AddShapeCircle(radius: 0.4, center: (0.5, 0.6), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.6, 0.25), offset: (0.2, 0.8), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.8, 0.8),  offset: (0.1, 0.2), group: CollisionGroups.ClickArea);
        }
    }
}