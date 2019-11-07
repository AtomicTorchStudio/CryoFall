namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System;

    public class ItemTobaccoAged : ProtoItemWithFreshness, IProtoItemOrganic
    {
        public override string Description => "Aged tobacco leaves. Can be made into fine cigars.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Aged tobacco";

        public ushort OrganicValue => 1;
    }
}