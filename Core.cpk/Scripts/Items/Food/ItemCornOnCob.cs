namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCornOnCob : ProtoItemFood
    {
        public override string Description =>
            "Who doesn't like hot corn on the cob. What is cob, anyway?"; // cob is the central part of the corn ear to which the kernels are attached :)

        public override float FoodRestore => 8;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Corn on the cob";

        public override ushort OrganicValue => 5;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFood;
        }
    }
}