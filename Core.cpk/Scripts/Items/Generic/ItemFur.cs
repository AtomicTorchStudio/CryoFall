namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemFur : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description =>
            "Animal fur. Could be useful in creating warm winter clothing.";

        public override string Name => "Fur";

        public ushort OrganicValue => 5;
    }
}