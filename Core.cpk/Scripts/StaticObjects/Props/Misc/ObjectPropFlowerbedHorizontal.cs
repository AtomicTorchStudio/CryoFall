namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropFlowerbedHorizontal : ProtoObjectProp
    {
        public override bool CanFlipSprite => true;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.85;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (3, 0.8), offset: (0, 0.1));
            AddHalfHeightWallHitboxes(data, width: 2.8, offsetX: 0.1);
        }
    }
}