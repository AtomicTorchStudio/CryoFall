namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.WallsWooden
{
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropWallWoodenV1 : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.7, 1), offset: (0.15, 0));
            AddFullHeightWallHitboxes(data, width: 0.7, offsetX: 0.15);
        }
    }
}