namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectCampfire
        : ProtoObjectManufacturer
            <ObjectManufacturerPrivateState,
                ObjectCampfire.ObjectCampfirePublicState,
                StaticObjectClientState>
    {
        private readonly TextureAtlasResource textureAtlasCampfireActive;

        private readonly TextureResource textureResourceCampfireBurned;

        public ObjectCampfire()
        {
            this.textureAtlasCampfireActive = new TextureAtlasResource(
                this.GenerateTexturePath() + "Fire",
                columns: 4,
                rows: 2,
                isTransparent: true);

            this.textureResourceCampfireBurned = new TextureResource(
                this.GenerateTexturePath() + "Burned");
        }

        public override byte ContainerFuelSlotsCount => 1;

        public override byte ContainerInputSlotsCount => 4;

        public override byte ContainerOutputSlotsCount => 4;

        public override string Description =>
            "Serves as a basic light source and, more importantly, allows for cooking simple recipes such as fried meat.";

        public override bool IsAutoSelectRecipe => false;

        public override bool IsAutoUnlocked => true;

        public override bool IsFuelProduceByproducts => true;

        public override string Name => "Campfire";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0;

        public override float StructurePointsMax => 75;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var spriteRenderer = data.ClientState.Renderer;
            var drawOrderOffsetY = 0.5;
            spriteRenderer.DrawOrderOffsetY = drawOrderOffsetY;

            var worldObject = data.GameObject;

            // setup light source
            var lightSource = ClientLighting.CreateLightSourceSpot(
                worldObject.ClientSceneObject,
                color: LightColors.WoodFiring,
                size: 11,
                spritePivotPoint: (0.5, 0.5),
                positionOffset: (0.5, 0.5));

            // add light flickering
            lightSource.SceneObject
                       .AddComponent<ClientComponentLightSourceEffectFlickering>()
                       .Setup(lightSource,
                              flickeringPercents: 10,
                              flickeringChangePercentsPerSecond: 50);

            // setup active animation
            this.ClientSetupManufacturerActiveAnimation(
                worldObject,
                data.PublicState,
                this.textureAtlasCampfireActive,
                positionOffset: (0, 0),
                drawOrderOffsetY: drawOrderOffsetY,
                frameDurationSeconds: 1 / 12.5,
                autoInverseAnimation: false,
                randomizeInitialFrame: true,
                onRefresh: isActive =>
                           {
                               // refresh primary sprite
                               spriteRenderer.TextureResource = this.GetTextureResource(worldObject);
                               // enable the light renderer when the fire animation is active
                               lightSource.IsEnabled = isActive;
                           });
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryFood>();

            build.StagesCount = 1;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemTwigs>(count: 10);
            build.AddStageRequiredItem<ItemStone>(count: 10);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemTwigs>(count: 1);
            repair.AddStageRequiredItem<ItemStone>(count: 1);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var publicState = data.PublicState;
            if (!publicState.IsBurned
                && data.PrivateState.FuelBurningState.FuelUseTimeRemainsSeconds > 0)
            {
                publicState.IsBurned = true;
            }
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.3, center: (0.35, 0.45))
                .AddShapeCircle(radius: 0.3, center: (0.65, 0.45))
                .AddShapeCircle(radius: 0.4, center: (0.5, 0.45), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 0.2), offset: (0.1, 0.85), group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(radius: 0.4, center: (0.5, 0.45), group: CollisionGroups.ClickArea);
        }

        private ITextureResource GetTextureResource(IStaticWorldObject worldObject)
        {
            var publicState = GetPublicState(worldObject);
            return publicState.IsBurned || publicState.IsActive
                       ? this.textureResourceCampfireBurned
                       : this.DefaultTexture;
        }

        public class ObjectCampfirePublicState : ObjectManufacturerPublicState
        {
            [SyncToClient]
            public bool IsBurned { get; set; }
        }
    }
}