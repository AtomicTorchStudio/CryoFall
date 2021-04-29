namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.WallsWooden
{
    public class ObjectPropWallWoodenJunctionR : ProtoPropWallWooden
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.85, 1), offset: (0.15, 0));

            AddFullHeightWallHitboxes(data, width: 0.85, offsetX: 0.15);
        }
    }
}