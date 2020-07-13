namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemMilk : ProtoItemFood
    {
        public override string Description => "Nutrient-rich milk. Can be consumed as-is or used in cooking.";

        public override float FoodRestore => 3;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Milk";

        public override ushort OrganicValue => 5;

        public override float WaterRestore => 15;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHealthyFood>(intensity: 0.05);
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrink;
        }
    }
}