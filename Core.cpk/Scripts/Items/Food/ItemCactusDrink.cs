namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCactusDrink : ProtoItemFood
    {
        public override string Description =>
            "Drink made from squeezed cactus flesh. Tastes like crap, but will quench your thirst when no other options are available.";

        public override float FoodRestore => 3;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Cactus drink";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 50;

        public override float WaterRestore => 15;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrink;
        }
    }
}