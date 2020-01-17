namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemLeather : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description => "Animal leather. Could be useful in creation of leather armor.";

        public override string Name => "Leather";

        public ushort OrganicValue => 3;
    }
}