namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectGateSteelH : ObjectGateSteel
    {
        public override bool? IsHorizontalDoorOnly => true;

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }
    }
}