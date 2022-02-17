namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.WallsWooden
{
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoPropWallWooden : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            // implemented in concrete classes
        }
    }
}