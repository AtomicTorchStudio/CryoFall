namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemOgurin : ProtoItemFood
    {
        public override string Description =>
            "Strange-looking ogurin fruits...quite tasteless. Genetically engineered from cucumbers.";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Ogurin fruit";

        public override ushort OrganicValue => 5;

        public override float WaterRestore => 10;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}