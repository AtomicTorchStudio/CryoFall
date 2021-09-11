namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemSulfurPowder : ProtoItemGeneric
    {
        public override string Description => "Pure sulfur in powdered form.";

        public override string Name => "Sulfur powder";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;
    }
}