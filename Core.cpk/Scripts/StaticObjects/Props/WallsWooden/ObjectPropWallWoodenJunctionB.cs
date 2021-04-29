namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.WallsWooden
{
    public class ObjectPropWallWoodenJunctionB : ProtoPropWallWooden
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 0.8));

            AddFullHeightWallHitboxes(data);
        }
    }
}