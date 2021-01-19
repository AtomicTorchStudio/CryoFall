namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;

    public class ObjectPropRoadDown : ObjectPropRoadUp
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var width = 0.56;
            data.PhysicsBody
                .AddShapeRectangle((width, 1.75), (3.0 - 2 - width - 1, 1.35), CollisionGroups.Default)
                .AddShapeRectangle((width, 1.75), (3.0 - 1, 1.35),             CollisionGroups.Default);
        }
    }
}