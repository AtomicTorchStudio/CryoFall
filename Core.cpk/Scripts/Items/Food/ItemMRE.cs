namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemMRE : ProtoItemFood
    {
        public override string Description =>
            "Industrially prepared meal containing several high-calorie dishes and necessary hydration to survive in the wilderness or during military operations.";

        public override float FoodRestore => 30;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        // MRE = meal ready to eat, military abbreviation
        public override string Name => "MRE";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 100;

        public override float WaterRestore => 30;
    }
}