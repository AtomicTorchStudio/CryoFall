namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemBones : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description => "Animal bones.";

        public override string Name => "Bones";

        public ushort OrganicValue => 10;
    }
}