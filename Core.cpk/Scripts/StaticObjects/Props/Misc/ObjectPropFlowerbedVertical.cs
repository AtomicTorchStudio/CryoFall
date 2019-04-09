namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropFlowerbedVertical : ProtoObjectProp
    {
        public override bool CanFlipSprite => true;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 2.5;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#",
                         "#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 2.5), offset: (0, 0.1));
        }
    }
}