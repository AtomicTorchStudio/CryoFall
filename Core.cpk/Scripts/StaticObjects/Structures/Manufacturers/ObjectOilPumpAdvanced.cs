namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectOilPumpAdvanced : ProtoObjectOilPump
    {
        private readonly TextureAtlasResource textureAtlasOilPumpActive;

        public ObjectOilPumpAdvanced()
        {
            this.textureAtlasOilPumpActive = new TextureAtlasResource(
                this.GenerateTexturePath() + "Active",
                columns: 8,
                rows: 2,
                isTransparent: true);
        }

        public override byte ContainerFuelSlotsCount => 0;

        public override string Description => GetProtoEntity<ObjectOilPump>().Description;

        // do not change, see electricity math model
        public override double ElectricityConsumptionPerSecondWhenActive => 4;

        public override double LiquidCapacity => 100;

        public override double LiquidProductionAmountPerSecond => 0.4;

        public override string Name => "Advanced oil pump";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 10000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var animatedSpritePositionOffset = (300 / (double)ScriptingConstants.TileSizeRealPixels,
                                                376 / (double)ScriptingConstants.TileSizeRealPixels);

            this.ClientSetupExtractorActiveAnimation(
                data.GameObject,
                data.PublicState,
                this.textureAtlasOilPumpActive,
                animatedSpritePositionOffset,
                frameDurationSeconds: 0.04,
                autoInverseAnimation: true);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 1.8;
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
            build.AddStageRequiredItem<ItemIngotSteel>(count: 10);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 5);
            build.AddStageRequiredItem<ItemCement>(count: 20);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 2);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            // use the oil pump active sound
            return base.PrepareSoundPresetObject().Clone().Replace(
                ObjectSound.Active,
                "Objects/Structures/" + nameof(ObjectOilPumpAdvanced) + "/Active");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((3.0, 2.2), (0, 0))
                .AddShapeRectangle((2.8, 1.7), (0.1, 0.7), CollisionGroups.HitboxMelee)
                .AddShapeRectangle((2.8, 1.6), (0.1, 0.8), CollisionGroups.HitboxRanged)
                .AddShapeRectangle((2.8, 2.8), (0.1, 0),   CollisionGroups.ClickArea);
        }
    }
}