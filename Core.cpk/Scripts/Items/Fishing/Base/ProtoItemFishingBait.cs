namespace AtomicTorch.CBND.CoreMod.Items.Fishing.Base
{
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class ProtoItemFishingBait : ProtoItemWithFreshness, IProtoItemFishingBait
    {
        protected ProtoItemFishingBait()
        {
            this.Icon = new TextureResource("Items/Fishing/" + this.GetType().Name);
        }

        public override ITextureResource Icon { get; }

        public override ushort MaxItemsPerStack => ItemStackSize.Small;
    }
}