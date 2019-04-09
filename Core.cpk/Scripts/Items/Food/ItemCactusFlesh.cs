namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCactusFlesh : ProtoItemFood
    {
        public override string Description =>
            "Juicy cactus flesh. Not exactly something you'd want to eat, but it will save you in a pinch.";

        public override float FoodRestore => 1;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override string Name => "Cactus flesh";

        public override ushort OrganicValue => 2;

        public override float WaterRestore => 1;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}