namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Bunker
{
    public class ObjectPropBunkerRoof : ProtoObjectProp
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            AddRectangleWithHitboxes(data, size: (1, 1.1));
        }
    }
}