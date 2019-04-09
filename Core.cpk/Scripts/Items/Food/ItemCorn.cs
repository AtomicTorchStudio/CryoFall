namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCorn : ProtoItemFood
    {
        public override string Description => "A tasty grain plant especially favored by horror writers.";

        public override float FoodRestore => 7;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Corn";

        public override ushort OrganicValue => 5;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}