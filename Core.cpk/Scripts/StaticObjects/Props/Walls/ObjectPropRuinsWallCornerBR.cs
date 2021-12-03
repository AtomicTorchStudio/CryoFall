namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Walls
{
    public class ObjectPropRuinsWallCornerBR : ProtoObjectProp
    {
        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.85, 0.75))
                .AddShapeRectangle(size: (0.7, 0.75), offset: (0.15, 0.5));
            AddHalfHeightWallHitboxes(data, width: 0.85, offsetY: -0.15);
        }
    }
}