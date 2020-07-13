namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropRoadTurnTR : ProtoObjectPropRoad
    {
        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("####",
                         "####",
                         "####",
                         "####");
        }
    }
}