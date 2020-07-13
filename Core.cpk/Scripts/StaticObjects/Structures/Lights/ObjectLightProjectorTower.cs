namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectLightProjectorTower
        : ProtoObjectLightElectrical
            <ObjectLightWithElectricityPrivateState,
                ObjectLightWithElectricityPublicState,
                ObjectLightProjectorTower.ClientState>
    {
        private static readonly Vector2D TextureResourceActiveLightOffset
            = (-102 / 256.0, 386 / 256.0);

        private ITextureResource textureResourceActiveLight;

        public override string Description =>
            "Ideal for large bases or military outposts, this tall projector tower can ensure perfect visibility in a wide area.";

        public override double ElectricityConsumptionPerSecondWhenActive => 0.2;

        public override Color LightColor => LightColors.ElectricCold;

        public override double LightSize => 30;

        public override string Name => "Projector tower";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 5000;

        protected override ITextureResource ClientCreateIcon()
        {
            var offsetY = -20;
            return ClientProceduralTextureHelper.CreateComposedTexture(
                "Composed " + this.Id,
                isTransparent: true,
                isUseCache: true,
                customSize: (462, (ushort)(685 + offsetY)),
                textureResourcesWithOffsets: new[]
                {
                    new TextureResourceWithOffset(this.DefaultTexture, offset: (103, offsetY)),
                    new TextureResourceWithOffset(this.textureResourceActiveLight)
                });
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var clientState = data.ClientState;
            var spriteRendererLightSprite = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                this.textureResourceActiveLight,
                drawOrder: DrawOrder.Light,
                positionOffset: TextureResourceActiveLightOffset);
            spriteRendererLightSprite.BlendMode = BlendMode.AdditiveNonPremultiplied;
            spriteRendererLightSprite.SortByWorldPosition = false;

            clientState.RendererLightSprite = spriteRendererLightSprite;

            base.ClientInitialize(data);

            clientState.RendererLight.PositionOffset += (0, 1);
        }

        protected override void ClientIsActiveChanged(ClientInitializeData data)
        {
            var isActive = data.PublicState.IsLightActive;

            var clientState = data.ClientState;
            clientState.RendererLight.IsEnabled = isActive;
            clientState.RendererLightSprite.IsEnabled = isActive;
        }

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
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemGlassRaw>(count: 10);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            build.AddStageRequiredItem<ItemWire>(count: 5);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemGlassRaw>(count: 5);
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var texturePath = GenerateTexturePath(thisType);
            this.textureResourceActiveLight = new TextureResource(texturePath + "Light");
            return new TextureResource(texturePath);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.8, 0.6), offset: (0.1, 0.1))
                .AddShapeRectangle(size: (0.5, 1.0), offset: (0.25, 0.3), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.5, 0.3), offset: (0.25, 0.95), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.5, 1.25), offset: (0.25, 0.3), group: CollisionGroups.ClickArea);
        }

        public class ClientState : ObjectLightClientState
        {
            public IComponentSpriteRenderer RendererLightSprite { get; set; }
        }
    }
}