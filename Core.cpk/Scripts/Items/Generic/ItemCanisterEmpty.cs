namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemCanisterEmpty : ProtoItemGeneric
    {
        public override string Description => "Empty metal canister. Can be used to store volatile liquids.";

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Empty canister";
    }
}