namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Environment
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectLavaCrack2 : ProtoObjectLavaCrack
    {
        public override double HeatRadiusMax => 6;

        public override double HeatRadiusMin => 3;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            ClientLighting.CreateLightSourceSpot(
                Client.Scene.GetSceneObject(data.GameObject),
                color: LightColors.Lava,
                size: (9, 10.5),
                positionOffset: (1, 0.75));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 2.0;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override ITextureAtlasResource PrepareTextureAtlasAnimation(
            out double frameDurationSeconds,
            out Vector2D drawPositionWorldOffset)
        {
            frameDurationSeconds = 1.8;
            drawPositionWorldOffset = (163 / 256.0, 73 / 256.0);
            return new TextureAtlasResource(this.GenerateTexturePath() + "Animation",
                                            columns: 3,
                                            rows: 1,
                                            isTransparent: true);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.45, center: (1.0, 0.7))
                .AddShapeCircle(radius: 0.4,  center: (0.9, 1.25));
        }
    }
}