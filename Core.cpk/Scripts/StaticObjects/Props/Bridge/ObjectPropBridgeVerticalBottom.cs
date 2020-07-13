namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Bridge
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropBridgeVerticalBottom : ProtoObjectPropPlatform
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
            const double yOffset = 0.4;
            data.PhysicsBody
                .AddShapeRectangle(size: (0.75, 2 - yOffset), offset: (-0.75, yOffset), group: CollisionGroups.Default)
                .AddShapeRectangle(size: (0.75, 2 - yOffset), offset: (2, yOffset),     group: CollisionGroups.Default);
        }
    }
}