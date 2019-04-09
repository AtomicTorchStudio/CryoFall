namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemDurian : ProtoItemFood
    {
        public override string Description =>
            "People either love or hate this tropical fruit, but you can't deny that it certainly has a unique flavor and smell to it.";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Durian";

        public override ushort OrganicValue => 3;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}