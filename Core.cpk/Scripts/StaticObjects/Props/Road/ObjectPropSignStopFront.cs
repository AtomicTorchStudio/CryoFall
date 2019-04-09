namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road
{
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropSignStopFront : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 0.25;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.4, 0.4), offset: (0.3, 0));
        }
    }
}