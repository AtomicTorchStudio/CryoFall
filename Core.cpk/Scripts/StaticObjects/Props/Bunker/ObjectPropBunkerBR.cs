namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Bunker
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropBunkerBR : ProtoObjectProp
    {
        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 2), offset: (0, 0));
        }
    }
}