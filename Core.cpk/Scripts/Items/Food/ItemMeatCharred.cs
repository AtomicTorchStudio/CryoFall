namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;

    public class ItemMeatCharred : ProtoItemFood
    {
        public override string Description =>
            "Meat prepared over open flame. Not the best way to prepare meat, but at least it's better than raw.";

        public override float FoodRestore => 8;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Fire-charred meat";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 50;

        public override float WaterRestore => -5;
    }
}