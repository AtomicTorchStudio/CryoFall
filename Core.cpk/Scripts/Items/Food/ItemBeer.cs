namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBeer : ProtoItemFood
    {
        public override string Description =>
            "Refreshing and cool. This light beer is brewed from high-quality grains.";

        public override float FoodRestore => 3; // Yes, beer has calories

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Beer";

        public override ushort OrganicValue => 0;

        public override float WaterRestore => 3; // doesn't hydrate much, because of alcohol

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectDrunk>(intensity: 0.20); // adds drunk status effect
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrinkAlcohol;
        }
    }
}