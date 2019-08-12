namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Building
{
    public class ObjectPropRuinsBuildingL : ProtoObjectProp
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            AddRectangleWithHitboxes(data, size: (0.9, 1), offset: (0.1, 0));
        }
    }
}