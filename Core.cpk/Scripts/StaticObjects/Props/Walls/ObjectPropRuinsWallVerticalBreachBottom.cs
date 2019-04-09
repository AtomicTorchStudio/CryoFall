namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Walls
{
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropRuinsWallVerticalBreachBottom : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.7, 0.75), offset: (0.15, 0.25));
        }
    }
}