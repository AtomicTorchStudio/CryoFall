namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemPainkiller : ProtoItemMedical
    {
        public override string Description =>
            "Painkiller can be used to completely remove and prevent any pain for the duration of its effect.";

        public override double MedicalToxicity => 0.1;

        public override string Name => "Painkiller";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillRemoveEffect<StatusEffectPain>()                        // removes all pain
                .WillAddEffect<StatusEffectProtectionPain>(intensity: 0.50); // adds pain blocking
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalTablets;
        }
    }
}