namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Bridge
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropBridgeVerticalTop : ProtoObjectPropPlatform
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (-2, 0);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            const double height = 1.3;
            data.PhysicsBody
                .AddShapeRectangle(size: (1, height), offset: (-1, 0), group: CollisionGroups.Default)
                .AddShapeRectangle(size: (1, height), offset: (2, 0),  group: CollisionGroups.Default);
        }
    }
}