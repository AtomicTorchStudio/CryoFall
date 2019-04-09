namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemOreIron : ProtoItemGeneric
    {
        public override string Description => "Can be smelted into pure iron.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Iron ore";
    }
}