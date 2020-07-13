namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Bridge
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropBridgeHorizontal : ProtoObjectPropPlatform
    {
        private ITextureResource frontTexture;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var frontRenderer = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                textureResource: this.frontTexture);

            this.ClientSetupRenderer(frontRenderer);
            frontRenderer.DrawOrder = DrawOrder.Default;
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (0, -2);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var folderPath = SharedGetRelativeFolderPath(thisType, typeof(ProtoObjectProp));
            var spritePath = $"StaticObjects/Props/{folderPath}/{thisType.Name}";
            this.frontTexture = new TextureResource(spritePath + "Front");
            return new TextureResource(spritePath);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((2, 0.7),    (0, 2),       CollisionGroups.Default)
                .AddShapeRectangle((0.7, 0.95), (0.65, 2),    CollisionGroups.Default)
                .AddShapeRectangle((2, 1),      (0, -1),      CollisionGroups.Default)
                .AddShapeRectangle((0.7, 1.4),  (0.65, -1.4), CollisionGroups.Default);
        }
    }
}