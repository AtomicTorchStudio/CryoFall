namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLightFloorLampSmall : ProtoObjectLightElectrical
    {
        public override string Description =>
            "Smaller version of the electric floor lamp. Quite bright for its size.";

        public override double ElectricityConsumptionPerSecondWhenActive => 0.1;

        public override Color LightColor => LightColors.ElectricCold;

        public override double LightSize => 19;

        public override string Name => "Small floor lamp";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 1200;

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

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemGlassRaw>(count: 5);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemGlassRaw>(count: 5);
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.25, center: (0.5, 0.45))
                .AddShapeRectangle(size: (0.35, 0.4), offset: (0.325, 0.75), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.3, 0.2),  offset: (0.35, 0.95),  group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.5, 1.25), offset: (0.25, 0.3),   group: CollisionGroups.ClickArea);
        }
    }
}