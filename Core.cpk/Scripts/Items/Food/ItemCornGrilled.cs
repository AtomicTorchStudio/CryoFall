namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCornGrilled : ProtoItemFood
    {
        public override string Description => "This grilled corn retains all of the sweetness and adds smoky flavors. Very tasty!";

        public override float FoodRestore => 8;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Grilled corn";

        public override ushort OrganicValue => 5;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFood;
        }
    }
}