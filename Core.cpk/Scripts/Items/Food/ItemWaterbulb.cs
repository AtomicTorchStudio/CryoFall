namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemWaterbulb : ProtoItemFood
    {
        public override string Description => "Fruit of waterbulb plant. Contains pure drinking water.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Waterbulb fruit";

        public override ushort OrganicValue => 2;

        public override float StaminaRestore => 5;

        public override float WaterRestore => 4;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrink;
        }
    }
}