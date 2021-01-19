namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemPotatoBakedWithMeat : ProtoItemFood
    {
        public override string Description =>
            "Fresh potatoes with a nice delicious crust and some meat on the side.";

        public override float FoodRestore => 23;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Potato baked with meat";

        public override ushort OrganicValue => 5;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFood;
        }
    }
}