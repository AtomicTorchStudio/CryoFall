namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemFishRoasted : ProtoItemFood
    {
        public override string Description =>
            "Very tender and juicy oven-roasted fish fillet.";

        public override float FoodRestore => 20;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Roasted fish";

        public override ushort OrganicValue => 10;
    }
}