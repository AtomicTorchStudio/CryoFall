namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System;

    public class ItemTobaccoDried : ProtoItemWithFreshness, IProtoItemOrganic
    {
        public override string Description =>
            "Dried tobacco leaves. Can be made into cheap cigars or further aged to produce fine cigars.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Dried tobacco";

        public ushort OrganicValue => 1;
    }
}