namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Walls
{
    public class ObjectPropRuinsWallCornerTR : ProtoObjectProp
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.85, 0.75));
            AddFullHeightWallHitboxes(data, width: 0.85, offsetY: -0.15);
        }
    }
}