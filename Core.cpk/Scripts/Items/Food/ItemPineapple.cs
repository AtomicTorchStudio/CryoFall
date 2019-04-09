namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemPineapple : ProtoItemFood
    {
        public override string Description =>
            "Sweet and bitter tropical fruit. Can be eaten raw or prepared in various dishes. Some say it even goes well on pizza.";

        public override float FoodRestore => 4;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Pineapple";

        public override ushort OrganicValue => 3;

        public override float WaterRestore => 2;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}