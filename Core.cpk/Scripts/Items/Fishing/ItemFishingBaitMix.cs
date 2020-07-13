namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class ItemFishingBaitMix : ProtoItemFishingBait, IProtoItemOrganic
    {
        public override string Description =>
            "Boilies are a special mixed bait combining many different ingredients, which makes it suitable for most types of fish.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override string Name => "Boilie bait";

        public ushort OrganicValue => 2;
    }
}