namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemDrinkSoft : ProtoItemFood
    {
        public override string Description =>
            "Soft drink that was voted to be the most popular beverage in the core sector for over seven centuries in a row.";

        public override float FoodRestore => 3;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Soft drink";

        public override ushort OrganicValue => 0;

        public override float StaminaRestore => 50;

        public override float WaterRestore => 30;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrinkCan;
        }
    }
}