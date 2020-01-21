namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemCannedMixedMeat : ProtoItemFood
    {
        public override string Description => "Canned meat of dubious origin. At least the packaging looks decent.";

        public override float FoodRestore => 25;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public override string Name => "Canned mixed meat";

        public override ushort OrganicValue => 0;
    }
}