namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBerriesViolet : ProtoItemFood
    {
        public override string Description => "Tasty violet berries.";

        public override float FoodRestore => 2;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override string Name => "Berries";

        public override ushort OrganicValue => 3;

        public override float StaminaRestore => 25;

        public override float WaterRestore => 3;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}