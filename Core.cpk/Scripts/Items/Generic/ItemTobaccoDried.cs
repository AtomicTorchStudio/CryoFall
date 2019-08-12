namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemTobaccoDried : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description =>
            "Dried tobacco leaves. Can be made into cheap cigars or further aged to produce fine cigars.";

        public override string Name => "Dried tobacco";

        public ushort OrganicValue => 1;
    }
}