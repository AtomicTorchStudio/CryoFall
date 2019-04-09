namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class ItemSaladFruit : ProtoItemFood, IProtoItemOrganic
    {
        public override string Description =>
            "Mixed fruit salad. Sweet, healthy and filling!";

        public override float FoodRestore => 18;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Fruit salad";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 50;

        public override float WaterRestore => 5;
    }
}