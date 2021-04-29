namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.WallsWooden
{
    public class ObjectPropWallWoodenJunctionL : ProtoPropWallWooden
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.85, 1));

            AddFullHeightWallHitboxes(data, width: 0.85, offsetX: 0);
        }
    }
}