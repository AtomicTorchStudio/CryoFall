namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemTwigs : ProtoItemGeneric, IProtoItemFuelSolid
    {
        public override string Description =>
            "Just some random twigs gathered from the ground or from a tree. Could be useful to craft simple tools or burn in a fire.";

        public double FuelAmount => 10;

        public override double GroundIconScale => 1.33f;

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Twigs";
    }
}