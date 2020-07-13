namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectSprinkler : ProtoObjectSprinkler
    {
        public override string Description => "Automatically waters adjacent farm plots at regular intervals.";

        public override uint ElectricityConsumptionPerWatering => 500;

        public override string Name => "Water sprinkler";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Glass;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 500;

        public override double WaterCapacity => 8 * this.WaterConsumptionPerWatering; // 8 waterings

        public override double WaterConsumptionPerWatering => 30; // 3 bottles per watering

        public override byte WateringDistance => 2;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0.5, 0.95);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (0.5, 0.15);
            renderer.DrawOrderOffsetY = 0.35;
        }

        protected override void PrepareConstructionConfigSprinkler(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryFood>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            build.AddStageRequiredItem<ItemGlassRaw>(count: 2);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override ITextureAtlasResource PrepareDefaultTextureAtlas(Type thisType)
        {
            return new TextureAtlasResource(GenerateTexturePath(thisType),
                                            columns: 11,
                                            rows: 1,
                                            isTransparent: true);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var offsetY = 0.3;
            data.PhysicsBody
                .AddShapeRectangle((0.6, 0.35), offset: (0.2, offsetY))
                .AddShapeRectangle((0.4, 0.7),  offset: (0.3, offsetY + 0.5),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.4, 0.25), offset: (0.3, offsetY + 0.85), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((0.4, 1.3),  offset: (0.3, offsetY),        group: CollisionGroups.ClickArea);
        }
    }
}