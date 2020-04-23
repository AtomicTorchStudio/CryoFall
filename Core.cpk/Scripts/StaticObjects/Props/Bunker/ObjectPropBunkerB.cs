namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Bunker
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropBunkerB : ProtoObjectProp
    {
        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((1, 2), null);
            AddHalfHeightWallHitboxes(data);
        }
    }
}