namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemMilkmelon : ProtoItemFood
    {
        public override string Description =>
            "Special genetically engineered plant that produces thick milky liquid similar in composition to actual milk.";

        public override float FoodRestore => 8;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Milkmelon";

        public override ushort OrganicValue => 5;

        public override float WaterRestore => 5;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}