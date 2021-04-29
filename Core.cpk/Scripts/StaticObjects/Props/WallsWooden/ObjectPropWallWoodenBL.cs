namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.WallsWooden
{
    public class ObjectPropWallWoodenBL : ProtoPropWallWooden
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.85, 0.8), offset: (0.15, 0.2));

            AddHalfHeightWallHitboxes(data, width: 0.85, offsetX: 0.15);
        }
    }
}