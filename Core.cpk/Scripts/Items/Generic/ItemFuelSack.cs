namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemFuelSack : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description =>
            "Special organ from an alien creature storing volatile liquid.";

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Fuel sack";

        public ushort OrganicValue => 10;
    }
}