namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Walls
{
    public class ObjectPropRuinsWallJunctionT : ProtoObjectProp
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 0.5),   offset: (0, 0))
                .AddShapeRectangle(size: (0.7, 0.5), offset: (0.15, 0.5));
        }
    }
}