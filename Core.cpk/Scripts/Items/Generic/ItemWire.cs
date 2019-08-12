namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemWire : ProtoItemGeneric
    {
        public override string Description => "Spool of insulated electrical wire.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Wire";
    }
}