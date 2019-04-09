namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemSand : ProtoItemGeneric
    {
        public override string Description => "Sand can be used to make glass or as construction material.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Sand";
    }
}