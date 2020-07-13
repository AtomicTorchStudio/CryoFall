namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemFishFilletRed : ProtoItemFood
    {
        public override string Description => "High-quality fish fillet, great ingredient for cooking.";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override string Name => "Red fish fillet";

        public override ushort OrganicValue => 5;
    }
}