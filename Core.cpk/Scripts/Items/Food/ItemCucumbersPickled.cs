namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using System;

    public class ItemCucumbersPickled : ProtoItemFood
    {
        public override string Description => "Sweet and crunchy pickles.";

        public override float FoodRestore => 8;

        public override TimeSpan FreshnessDuration => ExpirationDuration.LongLasting;

        public override string Name => "Pickled cucumbers";

        public override ushort OrganicValue => 5;

        public override float WaterRestore => 1;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFood;
        }

    }
}