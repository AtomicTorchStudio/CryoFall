namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectGateSteelV : ObjectGateSteel
    {
        public override bool? IsHorizontalDoorOnly => false;

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#");
        }
    }
}