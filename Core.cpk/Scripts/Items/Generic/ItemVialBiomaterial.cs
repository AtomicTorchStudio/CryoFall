namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cybernetics;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemVialBiomaterial : ProtoItemGeneric, IProtoItemWithReferenceTech
    {
        public override string Description =>
            "Full biomaterial vial. Used in cybernetics and medicine, especially to perform operations to install and remove implants.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Biomaterial vial";

        public TechNode ReferenceTech => Api.GetProtoEntity<TechNodeVialEmpty>();
    }
}