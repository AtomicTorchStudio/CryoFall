namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.GameApi.Resources;

    public readonly struct SkeletonSlotAttachment
    {
        public readonly string AttachmentName;

        public readonly TextureResource TextureResource;

        public SkeletonSlotAttachment(
            string skeletonName,
            string attachmentName,
            TextureResource textureResource)
        {
            this.AttachmentName = attachmentName;
            this.TextureResource = textureResource;
            this.SkeletonName = skeletonName;
        }

        public string SkeletonName { get; }

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}: {3}, {4}: {5}",
                                 nameof(this.SkeletonName),
                                 this.SkeletonName,
                                 nameof(this.AttachmentName),
                                 this.AttachmentName,
                                 nameof(this.TextureResource),
                                 this.TextureResource);
        }
    }
}