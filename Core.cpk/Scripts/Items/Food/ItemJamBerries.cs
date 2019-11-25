namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemJamBerries : ProtoItemFood
    {
        public override string Description => "Tasty jam made from berries.";

        public override float FoodRestore => 15;

        public override TimeSpan FreshnessDuration => ExpirationDuration.LongLasting;

        public override string Name => "Berry jam";

        public override ushort OrganicValue => 15;
    }
}