namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemCannedFish : ProtoItemFood
    {
        public override string Description => "Probably tuna? Get your omega-3 fix.";

        public override float FoodRestore => 25;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override string Name => "Canned fish";

        public override ushort OrganicValue => 0;
    }
}