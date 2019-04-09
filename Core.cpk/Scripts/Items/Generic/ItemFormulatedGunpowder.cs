namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemFormulatedGunpowder : ProtoItemGeneric
    {
        public override string Description =>
            "Specially formulated and shaped nitrocellulose granules designed for high-performance firearms.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Formulated gunpowder";
    }
}