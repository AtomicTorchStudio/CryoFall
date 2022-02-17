namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.BunkerWall
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropWallBunkerCornerBR : ProtoObjectProp
    {
        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##",
                         "##");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            AddRectangleWithHitboxes(data, size: (1.6, 2.0), offset: (0, 0));
            AddRectangleWithHitboxes(data, size: (1.2, 1.0), offset: (0.4, 2));
        }
    }
}