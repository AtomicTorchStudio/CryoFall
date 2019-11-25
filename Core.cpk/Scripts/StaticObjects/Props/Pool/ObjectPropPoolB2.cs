namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Pool
{
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropPoolB2 : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrder = DrawOrder.Floor;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody.AddShapeRectangle((1, 1));
        }
    }
}