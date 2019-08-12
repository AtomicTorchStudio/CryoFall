namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectFurnace : ProtoObjectManufacturer
    {
        private const float VerticalOffset = 0.4f; // vertical offset of the entire thing

        private readonly TextureAtlasResource textureAtlasFurnaceActive;

        public ObjectFurnace()
        {
            this.textureAtlasFurnaceActive = new TextureAtlasResource(
                this.GenerateTexturePath() + "Fire",
                columns: 4,
                rows: 2,
                isTransparent: false);
        }

        public override byte ContainerFuelSlotsCount => 1;

        public override byte ContainerInputSlotsCount => 4;

        public override byte ContainerOutputSlotsCount => 4;

        public override string Description =>
            "Can be used to smelt ore into metal ingots and process a few other raw materials.";

        public override bool IsAutoSelectRecipe => false;

        public override bool IsFuelProduceByproducts => true;

        public override string Name => "Furnace";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 1200;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;

            // setup light source
            var lightSource = ClientLighting.CreateLightSourceSpot(
                Client.Scene.GetSceneObject(worldObject),
                color: LightColors.WoodFiring,
                size: 3,
                spritePivotPoint: (0.5, 0.5),
                positionOffset: (1, 0.51f + VerticalOffset));

            // add light flickering
            lightSource.SceneObject
                       .AddComponent<ClientComponentLightSourceEffectFlickering>()
                       .Setup(lightSource,
                              flickeringPercents: 5,
                              flickeringChangePercentsPerSecond: 50);

            var animatedSpritePositionOffset = (81 / 128.0,
                                                VerticalOffset + 29 / 128.0);
            this.ClientSetupManufacturerActiveAnimation(
                worldObject,
                data.PublicState,
                this.textureAtlasFurnaceActive,
                animatedSpritePositionOffset,
                frameDurationSeconds: 0.08,
                autoInverseAnimation: false,
                drawOrderOffsetY: VerticalOffset,
                randomizeInitialFrame: true,
                onRefresh: isActive => { lightSource.IsEnabled = isActive; });

            data.ClientState.SoundEmitter.Volume = 0.7f;
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = VerticalOffset;
            renderer.PositionOffset = (0, VerticalOffset);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
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
            build.AddStageRequiredItem<ItemStone>(count: 20);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemStone>(count: 5);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.8, 1.2),
                                   offset: (0.1, 0 + VerticalOffset))
                .AddShapeRectangle(size: (1.6, 1.65),
                                   offset: (0.2, 0.1 + VerticalOffset),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.6, 0.5),
                                   offset: (0.2, 0.85 + VerticalOffset),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1.6, 1.65),
                                   offset: (0.2, 0.1 + VerticalOffset),
                                   group: CollisionGroups.ClickArea);
        }
    }
}