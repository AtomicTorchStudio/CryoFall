namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class ItemSaladVegetable : ProtoItemFood, IProtoItemOrganic
    {
        public override string Description =>
            "Mixed vegetable salad. Very healthy and surprisingly filling!";

        public override float FoodRestore => 25;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Vegetable salad";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 100;

        public override float WaterRestore => 10;
    }
}