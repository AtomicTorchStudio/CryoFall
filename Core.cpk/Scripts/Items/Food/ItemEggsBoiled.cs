namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemEggsBoiled : ProtoItemFood
    {
        public override string Description => "Boiled eggs. Boiled is healthier than fried!";

        public override float FoodRestore => 10;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override float HealthRestore => 0;

        public override string Name => "Boiled eggs";

        public override ushort OrganicValue => 5;
    }
}