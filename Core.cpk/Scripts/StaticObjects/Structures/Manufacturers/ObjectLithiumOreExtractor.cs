namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ObjectLithiumOreExtractor : ProtoObjectLithiumOreExtractor
    {
        private TextureAtlasResource textureAtlasActive;

        public override byte ContainerFuelSlotsCount => 1;

        public override string Description =>
            "Allows extraction of lithium salts from underground reservoir or directly from a geothermal spring for increased efficiency.";

        public override double LiquidCapacity => 100;

        public override double LiquidProductionAmountPerSecond => 0.25;

        public override string Name => "Lithium salts extractor";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 5000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            this.ClientSetupExtractorActiveAnimation(
                data.GameObject,
                data.PublicState,
                this.textureAtlasActive,
                positionOffset: (612 / (double)ScriptingConstants.TileSizeRealPixels,
                                 375 / (double)ScriptingConstants.TileSizeRealPixels),
                frameDurationSeconds: 0.04,
                autoInverseAnimation: true);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###",
                         "###");
        }

        protected override void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryIndustry>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemPlanks>(count: 10);
            build.AddStageRequiredItem<ItemCement>(count: 5);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 2);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemPlanks>(count: 10);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            this.textureAtlasActive = new TextureAtlasResource(
                this.GenerateTexturePath() + "Active",
                columns: 6,
                rows: 2,
                isTransparent: true);

            return base.PrepareDefaultTexture(thisType);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return base.PrepareSoundPresetObject().Clone().Replace(
                ObjectSound.Active,
                "Objects/Structures/" + nameof(ObjectLithiumOreExtractor) + "/Active");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((3.0, 2.25), (0, 0))
                .AddShapeRectangle((2.8, 1.7),  (0.1, 0.7), CollisionGroups.HitboxMelee)
                .AddShapeRectangle((2.8, 1.6),  (0.1, 0.8), CollisionGroups.HitboxRanged)
                .AddShapeRectangle((2.8, 2.8),  (0.1, 0),   CollisionGroups.ClickArea);
        }
    }
}