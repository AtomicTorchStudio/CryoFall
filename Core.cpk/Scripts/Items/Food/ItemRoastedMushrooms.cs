namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemRoastedMushrooms : ProtoItemFood
    {
        public override string Description => "Tasty roasted mushrooms.";

        public override float FoodRestore => 7;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Roasted mushrooms";

        public override ushort OrganicValue => 5;
    }
}