namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemMeatJerky : ProtoItemFood
    {
        public override string Description =>
            "Meat dried to make into jerky. Tasty, but dehydrates quite a bit. Stores well for a long time without spoiling.";

        public override float FoodRestore => 15;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override string Name => "Jerky";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 50;

        public override float WaterRestore => -10;
    }
}