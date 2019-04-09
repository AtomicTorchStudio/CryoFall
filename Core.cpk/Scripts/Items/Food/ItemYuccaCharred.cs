namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemYuccaCharred : ProtoItemFood
    {
        public override string Description => "This will keep you from starving to death in a pinch.";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Fire-charred yucca";

        public override ushort OrganicValue => 3;

        public override float WaterRestore => -1;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}