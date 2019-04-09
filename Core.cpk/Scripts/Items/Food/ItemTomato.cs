namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemTomato : ProtoItemFood
    {
        public override string Description => "Did you know that tomatoes are actually fruits and not vegetables?";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Tomato";

        public override ushort OrganicValue => 3;

        public override float WaterRestore => 5;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}