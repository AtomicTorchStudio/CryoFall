namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Bridge
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropBridgeHorizontalBrokenLeft : ObjectPropBridgeHorizontal
    {
        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((1, 0.7),         (0, 2),  CollisionGroups.Default)
                .AddShapeRectangle((1, 1),           (0, -1), CollisionGroups.Default)
                .AddShapeRectangle((1, 1 + 2 + 0.7), (1, -1), CollisionGroups.Default);
        }
    }
}