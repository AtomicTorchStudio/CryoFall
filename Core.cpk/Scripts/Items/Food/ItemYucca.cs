namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemYucca : ProtoItemFood
    {
        public override string Description => "Desert survival food. Should be cooked first.";

        public override float FoodRestore => 3;

        public override TimeSpan FreshnessDuration => ExpirationDuration.NonPerishable;

        public override string Name => "Yucca";

        public override ushort OrganicValue => 3;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}