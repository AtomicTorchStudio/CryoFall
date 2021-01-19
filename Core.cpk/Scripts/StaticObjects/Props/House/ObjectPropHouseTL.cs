namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.House
{
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropHouseTL : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrder = DrawOrder.OverDefault;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
        }
    }
}