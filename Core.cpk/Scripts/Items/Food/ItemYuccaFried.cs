namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemYuccaFried : ProtoItemFood
    {
        public override string Description =>
            "Well-cooked yucca fruit. Not the highest class cuisine, but it tastes fine and gives you valuable nutrients.";

        public override float FoodRestore => 10;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Fried yucca";

        public override ushort OrganicValue => 3;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}