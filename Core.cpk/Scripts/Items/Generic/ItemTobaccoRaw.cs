namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System;

    public class ItemTobaccoRaw : ProtoItemWithFreshness, IProtoItemOrganic
    {
        public override string Description => "Raw tobacco leaves and pods. Can be dried and made into cigars.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.LongLasting;

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Raw tobacco";

        public ushort OrganicValue => 2;
    }
}