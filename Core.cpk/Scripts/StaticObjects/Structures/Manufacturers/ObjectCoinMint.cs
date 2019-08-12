namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectCoinMint : ProtoObjectManufacturer
    {
        private TextureAtlasResource textureAtlasActive;

        public override byte ContainerFuelSlotsCount => 1;

        public override byte ContainerInputSlotsCount => 4;

        public override byte ContainerOutputSlotsCount => 4;

        public override string Description =>
            "Used to mint coins from different metals. Can also smelt coins back into ingots.";

        public override bool IsAutoSelectRecipe => false;

        public override bool IsFuelProduceByproducts => true;

        public override string Name => "Coin mint";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 5000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;

            // setup light source
            var lightSource = ClientLighting.CreateLightSourceSpot(
                Client.Scene.GetSceneObject(worldObject),
                color: LightColors.WoodFiring,
                size: 1,
                spritePivotPoint: (0.5, 0.5),
                positionOffset: (2.3, 0.9));

            var animatedSpritePositionOffset = (541 / 256.0, 171 / 256.0);
            this.ClientSetupManufacturerActiveAnimation(
                worldObject,
                data.PublicState,
                this.textureAtlasActive,
                animatedSpritePositionOffset,
                frameDurationSeconds: 1 / 12f,
                autoInverseAnimation: false,
                randomizeInitialFrame: true,
                onRefresh: isActive => { lightSource.IsEnabled = isActive; });
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.3;
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
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemBricks>(count: 10);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 4);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 4);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            // prepare atlas for active mode
            this.textureAtlasActive = new TextureAtlasResource(
                this.GenerateTexturePath() + "Fire",
                columns: 8,
                rows: 1,
                isTransparent: false);

            // return base texture
            return base.PrepareDefaultTexture(thisType);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (3, 0.7),    offset: (0, 0))
                .AddShapeRectangle(size: (2.6, 1.65), offset: (0.2, 0.1), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (2.6, 0.3),  offset: (0.2, 0.8), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (2.6, 1.65), offset: (0.2, 0.1), group: CollisionGroups.ClickArea);
        }
    }
}