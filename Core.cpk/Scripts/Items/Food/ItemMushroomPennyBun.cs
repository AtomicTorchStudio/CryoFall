namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemMushroomPennyBun : ProtoItemFood
    {
        public override string Description =>
            "Edible mushroom. Can be eaten raw, but tastes much better when cooked or added in a stew.";

        public override float FoodRestore => 3;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Penny bun";

        public override ushort OrganicValue => 5;
    }
}