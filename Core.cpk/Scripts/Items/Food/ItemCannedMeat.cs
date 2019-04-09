namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemCannedMeat : ProtoItemFood
    {
        public override string Description => "Canned meat of dubious origin. At least the packaging looks decent.";

        public override float FoodRestore => 10;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override string Name => "Canned meat";

        public override ushort OrganicValue => 0;
    }
}