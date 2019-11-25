namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Bunker
{
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropBunkerT : ProtoObjectProp
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