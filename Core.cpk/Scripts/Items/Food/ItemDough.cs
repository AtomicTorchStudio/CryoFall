namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemDough : ProtoItemFood
    {
        public override string Description => "Can be used in cooking for a variety of recipes.";

        public override float FoodRestore => 2;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Dough";

        public override ushort OrganicValue => 5;
    }
}