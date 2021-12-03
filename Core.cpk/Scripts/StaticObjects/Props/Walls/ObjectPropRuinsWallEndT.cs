namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Walls
{
    public class ObjectPropRuinsWallEndT : ProtoObjectProp
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.7, 0.75), offset: (0.15, 0));
            // not needed here
            //AddFullHeightWallHitboxes(data);
        }
    }
}