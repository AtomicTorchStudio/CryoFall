namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemCoinPenny : ProtoItemGeneric
    {
        public override string Description =>
            "Low denomination coins. These coins could be used to purchase cheap items in trading machines or traded with other survivors directly.";

        public override ushort MaxItemsPerStack => ItemStackSize.Huge;

        public override string Name => "Penny coin";
    }
}