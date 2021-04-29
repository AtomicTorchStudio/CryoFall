namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemFuelCellEmpty : ProtoItemGeneric
    {
        public override string Description =>
            "Empty fuel cell used to power various vehicles. Must be filled first.";

        public override ushort MaxItemsPerStack => 1;

        public override string Name => "Empty fuel cell";
    }
}