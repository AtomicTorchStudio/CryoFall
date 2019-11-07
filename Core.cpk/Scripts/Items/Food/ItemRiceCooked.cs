namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemRiceCooked : ProtoItemFood
    {
        public override string Description => "Good to go as-is or as part of a more complex meal.";

        public override float FoodRestore => 6;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Cooked rice";

        public override ushort OrganicValue => 3;
    }
}