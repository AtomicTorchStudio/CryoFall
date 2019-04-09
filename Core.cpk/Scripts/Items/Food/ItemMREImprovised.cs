namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemMREImprovised : ProtoItemFood
    {
        public override string Description =>
            "This meal is prepared with a few simple ingredients and salt as the main preservative. Stores well for a long time without spoiling.";

        public override float FoodRestore => 15;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        // MRE = meal ready to eat, military abbreviation
        public override string Name => "Improvised MRE";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 50;

        public override float WaterRestore => -5;
    }
}