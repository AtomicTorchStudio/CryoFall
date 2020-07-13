namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectMineralProcessingPlant : ProtoObjectManufacturer
    {
        private TextureAtlasResource textureAtlasActiveAnimation;

        public override byte ContainerFuelSlotsCount => 0;

        public override byte ContainerInputSlotsCount => 8;

        public override byte ContainerOutputSlotsCount => 8;

        public override ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new ElectricityThresholdsPreset(startupPercent: 30,
                                               shutdownPercent: 20);

        public override string Description =>
            "This processing plant pulverizes raw ore and sorts different ore fractions to increase overall smelting yield.";

        public override double ElectricityConsumptionPerSecondWhenActive => 4;

        public override bool IsAutoSelectRecipe => false;

        public override bool IsFuelProduceByproducts => false;

        public override string Name => "Mineral processing plant";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 10000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var animatedSpritePositionOffset = (81 / (double)ScriptingConstants.TileSizeRealPixels,
                                                124 / (double)ScriptingConstants.TileSizeRealPixels);
            this.ClientSetupManufacturerActiveAnimation(
                data.GameObject,
                data.PublicState,
                this.textureAtlasActiveAnimation,
                animatedSpritePositionOffset,
                frameDurationSeconds: 1 / 20.0,
                autoInverseAnimation: false,
                randomizeInitialFrame: true);

            data.ClientState.SoundEmitter.CustomMaxDistance = 6f;
            data.ClientState.SoundEmitter.Volume = 0.5f;
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 0.5;
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
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            build.AddStageRequiredItem<ItemWire>(count: 5);
            build.AddStageRequiredItem<ItemPlastic>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            repair.AddStageRequiredItem<ItemWire>(count: 2);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var texturePath = GenerateTexturePath(thisType);
            this.textureAtlasActiveAnimation = new TextureAtlasResource(texturePath + "Active",
                                                                        columns: 4,
                                                                        rows: 1,
                                                                        isTransparent: true);
            return new TextureResource(texturePath);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2.8, 2.0),  offset: (0.1, 0))
                .AddShapeRectangle(size: (2.6, 2.45), offset: (0.2, 0.1),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (2.6, 1.4),  offset: (0.2, 0.85), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (2.6, 2.45), offset: (0.2, 0.1),  group: CollisionGroups.ClickArea);
        }
    }
}