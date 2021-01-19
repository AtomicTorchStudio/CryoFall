namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemFlowerYellow : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description => "Useful for extracting oil used in cooking or as a decoration.";

        public override string Name => "Yellow flower";

        public ushort OrganicValue => 2;
    }
}