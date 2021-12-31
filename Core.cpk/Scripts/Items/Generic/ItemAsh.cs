namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemAsh : ProtoItemGeneric
    {
        public override string Description => "Ash left after burning wood.";

        public override string Name => "Ash";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;
    }
}