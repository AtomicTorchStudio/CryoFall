namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemOreCopper : ProtoItemGeneric
    {
        public override string Description => "Can be smelted into pure copper.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Copper ore";
    }
}