namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemEggsFried : ProtoItemFood
    {
        public override string Description => "Fried eggs. Sunny side up.";

        public override float FoodRestore => 15;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Fried eggs";

        public override ushort OrganicValue => 5;
    }
}