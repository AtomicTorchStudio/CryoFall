namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBanana : ProtoItemFood
    {
        /// <summary>
        /// A quote by Vilayanur S. Ramachandran
        /// </summary>
        public override string Description =>
            "Any ape can reach for a banana, but only humans can reach for the stars.";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Banana";

        public override ushort OrganicValue => 3;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}