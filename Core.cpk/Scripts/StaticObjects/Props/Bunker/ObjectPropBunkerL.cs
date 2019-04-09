namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Bunker
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropBunkerL : ProtoObjectProp
    {
        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 1), offset: (0, 0));
        }
    }
}