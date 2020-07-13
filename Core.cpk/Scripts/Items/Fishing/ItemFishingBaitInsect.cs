namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class ItemFishingBaitInsect : ProtoItemFishingBait, IProtoItemOrganic
    {
        public override string Description => "Cheap and easy bait, ideal for catching smaller freshwater fish.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override string Name => "Insect bait";

        public ushort OrganicValue => 2;
    }
}