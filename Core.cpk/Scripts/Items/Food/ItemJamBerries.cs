namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemJamBerries : ProtoItemFood
    {
        public override string Description => "Tasty jam made from berries.";

        public override float FoodRestore => 12;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override string Name => "Berry jam";

        public override ushort OrganicValue => 15;

        public override float StaminaRestore => 100;
    }
}