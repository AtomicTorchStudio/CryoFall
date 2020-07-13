namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemWine : ProtoItemFood
    {
        public override string Description =>
            "Majestic nectar made from the finest grapes (or the equivalent thereof). Exquisite taste and aroma.";

        public override float FoodRestore => 3; // Yes, wine has calories

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        // Restores 3 hp instantly, but given that you cannot drink more than a couple of bottles, it cannot be abused.
        public override float HealthRestore => 3;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Wine";

        public override ushort OrganicValue => 0;

        public override float WaterRestore => 3; // doesn't hydrate much, because of alcohol

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectDrunk>(intensity: 0.25);
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrinkAlcohol;
        }
    }
}