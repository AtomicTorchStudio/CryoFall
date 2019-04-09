namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemBread : ProtoItemFood
    {
        public override string Description => "Loaf of white bread. Look at all the carbs!";

        public override float FoodRestore => 15;

        public override TimeSpan FreshnessDuration => ExpirationDuration.NonPerishable;

        public override float HealthRestore => 1;

        public override string Name => "Bread loaf";

        public override ushort OrganicValue => 5;

        public override float StaminaRestore => 50;
    }
}