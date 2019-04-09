namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropCraterBig : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 3;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup(
                "####",
                "####",
                "####");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.9,  center: (1.35, 1.05))
                .AddShapeCircle(radius: 0.8,  center: (2.0, 1.45))
                .AddShapeCircle(radius: 0.65, center: (2.5, 1.8));
        }
    }
}