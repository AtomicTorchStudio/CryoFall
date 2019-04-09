namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropCraterRound : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 2;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.5,  center: (0.5, 1.0))
                .AddShapeCircle(radius: 0.7,  center: (0.85, 1.0))
                .AddShapeCircle(radius: 0.8,  center: (1.5, 1.0))
                .AddShapeCircle(radius: 0.65, center: (2.15, 1.0))
                .AddShapeCircle(radius: 0.5,  center: (2.5, 1.0));
        }
    }
}