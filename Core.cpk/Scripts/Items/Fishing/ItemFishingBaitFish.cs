namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class ItemFishingBaitFish : ProtoItemFishingBait, IProtoItemOrganic
    {
        public override string Description =>
            "Cut bait are simply pieces of smaller fish, making them ideal for catching large predatory fish.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override string Name => "Cut bait";

        public ushort OrganicValue => 2;
    }
}