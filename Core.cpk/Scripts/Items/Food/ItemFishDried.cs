namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemFishDried : ProtoItemFood
    {
        public override string Description =>
            "Dried and salted fish, great with beer. Stores well for a long time without spoiling.";

        public override float FoodRestore => 10;

        public override TimeSpan FreshnessDuration => ExpirationDuration.LongLasting;

        public override string Name => "Dried fish";

        public override ushort OrganicValue => 10;

        public override float WaterRestore => -5;
    }
}