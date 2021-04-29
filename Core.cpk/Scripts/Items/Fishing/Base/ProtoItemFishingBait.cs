namespace AtomicTorch.CBND.CoreMod.Items.Fishing.Base
{
    public abstract class ProtoItemFishingBait : ProtoItemWithFreshness, IProtoItemFishingBait
    {
        public override ushort MaxItemsPerStack => ItemStackSize.Small;
    }
}