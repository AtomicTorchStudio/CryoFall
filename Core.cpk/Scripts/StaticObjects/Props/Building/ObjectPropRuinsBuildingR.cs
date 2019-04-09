namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Building
{
    public class ObjectPropRuinsBuildingR : ProtoObjectProp
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 1), offset: (0, 0));
        }
    }
}