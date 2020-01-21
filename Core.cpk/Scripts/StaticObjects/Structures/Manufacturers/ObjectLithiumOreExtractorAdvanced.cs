namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLithiumOreExtractorAdvanced : ProtoObjectLithiumOreExtractor
    {
        private readonly TextureAtlasResource textureAtlasActive1;

        private readonly TextureAtlasResource textureAtlasActive2;

        public ObjectLithiumOreExtractorAdvanced()
        {
            var texturePath = this.GenerateTexturePath();
            this.textureAtlasActive1 = new TextureAtlasResource(
                texturePath + "Active1",
                columns: 8,
                rows: 1,
                isTransparent: true);

            this.textureAtlasActive2 = new TextureAtlasResource(
                texturePath + "Active2",
                columns: 8,
                rows: 1,
                isTransparent: true);
        }

        public override byte ContainerFuelSlotsCount => 0;

        public override string Description => GetProtoEntity<ObjectLithiumOreExtractor>().Description;

        // do not change, see electricity math model
        public override double ElectricityConsumptionPerSecondWhenActive => 4;

        public override double LiquidCapacity => 100;

        public override double LiquidProductionAmountPerSecond => 2;

        public override string Name => "Advanced lithium salts extractor";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 10000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var worldObject = data.GameObject;
            var publicState = data.PublicState;

            this.ClientSetupExtractorActiveAnimation(
                worldObject,
                publicState,
                this.textureAtlasActive1,
                positionOffset: (640 / (double)ScriptingConstants.TileSizeRealPixels,
                                 351 / (double)ScriptingConstants.TileSizeRealPixels),
                frameDurationSeconds: 0.04,
                autoInverseAnimation: true,
                playAnimationSounds: false);

            this.ClientSetupExtractorActiveAnimation(
                worldObject,
                publicState,
                this.textureAtlasActive2,
                positionOffset: (134 / (double)ScriptingConstants.TileSizeRealPixels,
                                 417 / (double)ScriptingConstants.TileSizeRealPixels),
                frameDurationSeconds: 0.03,
                autoInverseAnimation: false,
                playAnimationSounds: false);

            var soundEmitter = this.ClientCreateActiveStateSoundEmitterComponent(worldObject);

            publicState.ClientSubscribe(_ => _.IsActive,
                                        _ => RefreshActiveState(),
                                        data.ClientState);

            RefreshActiveState();

            void RefreshActiveState()
            {
                soundEmitter.IsEnabled = publicState.IsActive;
            }
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
            build.AddStageRequiredItem<ItemCement>(count: 10);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 2);
            

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
            return base.PrepareSoundPresetObject().Clone().Replace(
                ObjectSound.Active,
                "Objects/Structures/" + nameof(ObjectLithiumOreExtractorAdvanced) + "/Active");
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