namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemSalami : ProtoItemFood
    {
        public override string Description =>
            "Cured sausage made with specially prepared meat and spices.";

        public override float FoodRestore => 25;

        public override TimeSpan FreshnessDuration => ExpirationDuration.LongLasting;

        public override string Name => "Salami";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 25;
    }
}