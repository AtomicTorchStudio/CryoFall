namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.WallsWooden
{
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropWallWoodenX : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 1));

            AddFullHeightWallHitboxes(data);
        }
    }
}