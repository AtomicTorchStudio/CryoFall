namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemTobaccoRaw : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description => "Raw tobacco leaves and pods. Can be dried and made into cigars.";

        public override string Name => "Raw tobacco";

        public ushort OrganicValue => 2;
    }
}