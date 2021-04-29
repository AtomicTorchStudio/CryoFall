namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.WallsWooden
{
    public class ObjectPropWallWoodenX : ProtoPropWallWooden
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 1));

            AddFullHeightWallHitboxes(data);
        }
    }
}