namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Building
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropRuinsBuildingRoofHole : ProtoObjectProp
    {
        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2, 2), offset: (0, 0));
        }
    }
}