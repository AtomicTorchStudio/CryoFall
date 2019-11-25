namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Environment
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectLavaCrack3 : ProtoObjectLavaCrack
    {
        public override double HeatRadiusMax => 6;

        public override double HeatRadiusMin => 3;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            ClientLighting.CreateLightSourceSpot(
                data.GameObject.ClientSceneObject,
                color: LightColors.Lava,
                size: (12, 10),
                positionOffset: (1.5, 1));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 2.0;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###");
        }

        protected override ITextureAtlasResource PrepareTextureAtlasAnimation(
            out double frameDurationSeconds,
            out Vector2D drawPositionWorldOffset)
        {
            frameDurationSeconds = 1.8;
            drawPositionWorldOffset = (98 / 256.0, 108 / 256.0);
            return new TextureAtlasResource(this.GenerateTexturePath() + "Animation",
                                            columns: 3,
                                            rows: 1,
                                            isTransparent: true);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.6, center: (1.0, 1.0))
                .AddShapeCircle(radius: 0.5, center: (1.8, 1.1))
                .AddShapeCircle(radius: 0.2, center: (2.4, 0.9));
        }
    }
}