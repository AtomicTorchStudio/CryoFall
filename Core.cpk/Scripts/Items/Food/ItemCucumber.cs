namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCucumber : ProtoItemFood
    {
        public override string Description => "A watery plant known to instill fear in cats.";

        public override float FoodRestore => 2;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Cucumber";

        public override ushort OrganicValue => 3;

        public override float WaterRestore => 4;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}