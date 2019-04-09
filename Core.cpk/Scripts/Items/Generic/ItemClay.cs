namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemClay : ProtoItemGeneric
    {
        public override string Description => "Clay is a naturally occurring substance useful in construction.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Clay";
    }
}