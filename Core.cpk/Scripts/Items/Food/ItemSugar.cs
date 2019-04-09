namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemSugar : ProtoItemFood, IProtoItemOrganic
    {
        public override string Description =>
            "Sweet and white. Can be used in a variety of foods. Eating it raw is possible, but not a particularly good idea.";

        public override float FoodRestore => 2;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override string Name => "Sugar";

        public override ushort OrganicValue => 3;

        public override float WaterRestore => -2;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodCrunchy;
        }
    }
}