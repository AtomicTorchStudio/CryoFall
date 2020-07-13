namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemFishFilletWhite : ProtoItemFood
    {
        public override string Description => GetProtoEntity<ItemFishFilletRed>().Description;

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override string Name => "White fish fillet";

        public override ushort OrganicValue => 5;
    }
}