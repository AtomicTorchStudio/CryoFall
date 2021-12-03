namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Walls
{
    public class ObjectPropRuinsWallEndB : ProtoObjectProp
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.7, 1), offset: (0.15, 0));
            AddFullHeightWallHitboxes(data, width: 0.7, offsetX: 0.15, offsetY: -0.15);
        }
    }
}