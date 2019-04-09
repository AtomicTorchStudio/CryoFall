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

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Tincture";

        public override ushort OrganicValue => 0;

        public override float WaterRestore => -5; // dehydrates

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            // some alcohol
            data.Character.ServerAddStatusEffect<StatusEffectDrunk>(intensity: 0.2); // 2 minutes

            // adds small health regeneration
            data.Character.ServerAddStatusEffect<StatusEffectHealingSlow>(intensity: 0.25); // 25 seconds

            // remove toxins
            data.Character.ServerRemoveStatusEffectIntensity<StatusEffectToxins>(intensityToRemove: 0.1);

            // remove radiation
            data.Character.ServerRemoveStatusEffectIntensity<StatusEffectRadiationPoisoning>(intensityToRemove: 0.1);

            base.ServerOnEat(data);
        }
    }
}