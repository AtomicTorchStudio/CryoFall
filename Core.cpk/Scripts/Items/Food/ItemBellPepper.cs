namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBellPepper : ProtoItemFood
    {
        public override string Description => "A poor yet tasty imitation of a real pepper. Great in many dishes.";

        public override float FoodRestore => 4;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Bell pepper";

        public override ushort OrganicValue => 3;

        public override float WaterRestore => 2;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }
    }
}