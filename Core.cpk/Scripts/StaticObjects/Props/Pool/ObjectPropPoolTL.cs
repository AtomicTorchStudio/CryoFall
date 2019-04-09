namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Pool
{
    public class ObjectPropPoolTL : ProtoObjectProp
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 1), offset: (0, 0));
        }
    }
}