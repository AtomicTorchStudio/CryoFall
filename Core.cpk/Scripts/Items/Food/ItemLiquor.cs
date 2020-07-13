namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemLiquor : ProtoItemFood
    {
        public override string Description =>
            "Beautiful ruby-colored fruit liquor. Goes down easy despite high alcohol content.";

        public override float FoodRestore => 5; // Yes, alcohol has calories

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Liquor";

        public override ushort OrganicValue => 0;

        public override float WaterRestore => -4; // dehydrates

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectDrunk>(intensity: 0.35)
                .WillAddEffect<StatusEffectHealingSlow>(intensity: 0.10);
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrinkAlcohol;
        }
    }
}