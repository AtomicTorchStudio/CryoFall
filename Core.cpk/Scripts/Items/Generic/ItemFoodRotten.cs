namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemFoodRotten : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description => "This food has spoiled completely.";

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Rotten food";

        public ushort OrganicValue => 2;
    }
}