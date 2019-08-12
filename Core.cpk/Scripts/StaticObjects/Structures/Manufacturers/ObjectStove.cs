namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectStove : ProtoObjectManufacturer
    {
        private readonly TextureAtlasResource textureAtlasOvenActive;

        public ObjectStove()
        {
            this.textureAtlasOvenActive = new TextureAtlasResource(
                this.GenerateTexturePath() + "Fire",
                columns: 4,
                rows: 2,
                isTransparent: false);
        }

        public override byte ContainerFuelSlotsCount => 1;

        public override byte ContainerInputSlotsCount => 4;

        public override byte ContainerOutputSlotsCount => 4;

        public override string Description =>
            "Used for most cooking activities; can produce much better meals than a campfire.";

        public override bool IsAutoSelectRecipe => false;

        public override bool IsFuelProduceByproducts => true;

        public override string Name => "Cooking stove";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 400;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            // setup light source
            var lightSource = ClientLighting.CreateLightSourceSpot(
                Client.Scene.GetSceneObject(data.GameObject),
                color: LightColors.WoodFiring,
                size: 1.25f,
                spritePivotPoint: (0.5, 0.5),
                positionOffset: (1, 1));

            // add light flickering
            lightSource.SceneObject
                       .AddComponent<ClientComponentLightSourceEffectFlickering>()
                       .Setup(lightSource,
                              flickeringPercents: 5,
                              flickeringChangePercentsPerSecond: 50);

            var animatedSpritePositionOffset = (85 / 128.0, 111 / 128.0);
            this.ClientSetupManufacturerActiveAnimation(
                data.GameObject,
                data.PublicState,
                this.textureAtlasOvenActive,
                animatedSpritePositionOffset,
                frameDurationSeconds: 0.08,
                autoInverseAnimation: false,
                randomizeInitialFrame: true,
                onRefresh: isActive => { lightSource.IsEnabled = isActive; });
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryFood>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemStone>(count: 5);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemStone>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.8, 0.8),
                                   offset: (0.1, 0))
                .AddShapeRectangle(size: (1.6, 1.3),
                                   offset: (0.2, 0),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.5, 0.3),
                                   offset: (0.25, 0.85),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1.5, 1.4),
                                   offset: (0.25, 0),
                                   group: CollisionGroups.ClickArea);
        }
    }
}