namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.BunkerWall
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropWallBunkerEndR : ProtoObjectProp
    {
        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            AddRectangleWithHitboxes(data, size: (1, 2.0), offset: (0, 0));
        }
    }
}