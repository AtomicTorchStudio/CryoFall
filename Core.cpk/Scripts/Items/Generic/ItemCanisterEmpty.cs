namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemCanisterEmpty : ProtoItemGeneric, IProtoItemWithReferenceTech
    {
        public override string Description => "Can be used to store volatile liquids.";

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Empty canister";

        public TechNode ReferenceTech => Api.GetProtoEntity<TechNodeCanisterEmpty>();
    }
}