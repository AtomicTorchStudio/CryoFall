namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemTobaccoAged : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description => "Aged tobacco leaves. Can be made into fine cigars.";

        public override string Name => "Aged tobacco";

        public ushort OrganicValue => 1;
    }
}