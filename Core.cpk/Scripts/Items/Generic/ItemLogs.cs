namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemLogs : ProtoItemGeneric, IProtoItemFuelSolid
    {
        public override string Description => "Great as building material. Also great for BBQ.";

        public double FuelAmount => 50;

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Wood logs";
    }
}