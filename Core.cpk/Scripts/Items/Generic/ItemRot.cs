namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemRot : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description =>
            "Food or other organic matter has completely spoiled and turned into this rotten mush.";

        public override string Name => "Rot";

        public ushort OrganicValue => 2;
    }
}