namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectCistern : ProtoObjectBarrel
    {
        public override string Description => "Huge cistern for long-term storage of large volumes of liquids.";

        public override ushort LiquidCapacity => 10000;

        public override string Name => "Cistern";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 10000;

        protected override void ClientSetupLiquidTypeSpriteRenderer(IComponentSpriteRenderer renderer)
        {
            var offsetY = 1.15;
            renderer.PositionOffset = (0.975, y: offsetY);
            renderer.DrawOrderOffsetY = -offsetY;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
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
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            build.AddStageRequiredItem<ItemCement>(count: 10);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 3);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier2);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2.0, 1.0), offset: (0.0, 0.1), group: CollisionGroups.Default)
                .AddShapeRectangle(size: (1.6, 1.5), offset: (0.2, 0.2), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.6, 1.5), offset: (0.2, 0.2), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1.6, 1.5), offset: (0.2, 0.2), group: CollisionGroups.ClickArea);
        }
    }
}