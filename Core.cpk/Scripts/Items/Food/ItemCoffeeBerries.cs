namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCoffeeBerries : ProtoItemFood
    {
        public override string Description => "Can be eaten as-is or roasted in the oven to make coffee beans.";

        public override float FoodRestore => 2;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override string Name => "Coffee berries";

        public override ushort OrganicValue => 3;

        public override float WaterRestore => 1;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}