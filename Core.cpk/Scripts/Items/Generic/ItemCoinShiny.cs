namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemCoinShiny : ProtoItemGeneric
    {
        public override string Description =>
            "High denomination coins. These coins could be used to purchase items in trading machines or traded with other survivors directly.";

        public override ushort MaxItemsPerStack => ItemStackSize.Huge;

        public override string Name => "Shiny coin";
    }
}