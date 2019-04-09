namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemLeaf : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description => "A leaf... Probably from a tree.";

        public override string Name => "Leaf";

        public ushort OrganicValue => 1;
    }
}