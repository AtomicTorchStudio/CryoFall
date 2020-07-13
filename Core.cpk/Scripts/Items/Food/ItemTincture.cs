namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemTincture : ProtoItemFood
    {
        public override string Description =>
            "This herbal tincture offers a variety of positive effects, including improving health.";

        public override float FoodRestore => 4; // Yes, alcohol has calories

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Tincture";

        public override ushort OrganicValue => 0;

        public override float WaterRestore => -5; // dehydrates

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectDrunk>(intensity: 0.20)
                .WillAddEffect<StatusEffectHealingSlow>(intensity: 0.25)
                .WillRemoveEffect<StatusEffectToxins>(intensityToRemove: 0.10)
                .WillRemoveEffect<StatusEffectRadiationPoisoning>(intensityToRemove: 0.10);
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric;
        }
    }
}