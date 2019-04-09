namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemVialBiomaterial : ProtoItemGeneric
    {
        public override string Description =>
            "Full biomaterial vial. Used in cybernetics and medicine, especially to perform operations to install and remove implants.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Biomaterial vial";
    }
}