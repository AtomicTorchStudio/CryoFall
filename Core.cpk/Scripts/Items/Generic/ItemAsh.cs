namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemAsh : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description => "Ash left after burning wood.";

        public override string Name => "Ash";

        public ushort OrganicValue => 1;
    }
}