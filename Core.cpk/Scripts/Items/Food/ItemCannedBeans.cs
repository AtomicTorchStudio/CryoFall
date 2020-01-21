namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemCannedBeans : ProtoItemFood
    {
        public override string Description => "Not the best food around, but could keep you from succumbing to hunger.";

        public override float FoodRestore => 25;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public override string Name => "Canned beans";

        public override ushort OrganicValue => 0;
    }
}