namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemCanisterEmpty : ProtoItemGeneric, IProtoItemWithReferenceTech
    {
        public override string Description => "Empty metal canister. Can be used to store volatile liquids.";

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Empty canister";

        public TechNode ReferenceTech => Api.GetProtoEntity<TechNodeCanisterEmpty>();
    }
}