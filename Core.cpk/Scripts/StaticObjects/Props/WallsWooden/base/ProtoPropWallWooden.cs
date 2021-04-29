using AtomicTorch.CBND.CoreMod.StaticObjects.Props;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.WallsWooden
{
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
