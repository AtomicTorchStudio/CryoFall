namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLightFloorLampLarge : ProtoObjectLightElectrical
    {
        public override string Description =>
            "Electric floor lamp. Produces extremely powerful white light that can easily illuminate even a large room.";

        public override double ElectricityConsumptionPerSecondWhenActive => 1;

        public override Color LightColor => LightColors.ElectricCold;

        public override double LightSize => 22;

        public override string Name => "Large floor lamp";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 800;

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
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 2);
            build.AddStageRequiredItem<ItemGlassRaw>(count: 4);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
            repair.AddStageRequiredItem<ItemGlassRaw>(count: 2);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.25, center: (0.5, 0.45))
                .AddShapeRectangle(size: (0.45, 0.4),  offset: (0.275, 0.75), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.4, 0.2),  offset: (0.3, 0.95), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.5, 1.25), offset: (0.25, 0.3),  group: CollisionGroups.ClickArea);
        }
    }
}