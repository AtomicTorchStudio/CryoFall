namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.WallsWooden
{
    public class ObjectPropWallWoodenGate : ProtoPropWallWooden
    {
        public override bool CanFlipSprite => true;

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 0.6), offset: (0, 0.2));

            AddHalfHeightWallHitboxes(data, width: 1);
        }
    }
}