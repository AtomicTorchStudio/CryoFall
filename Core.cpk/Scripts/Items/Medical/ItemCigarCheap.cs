namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCigarCheap : ProtoItemMedical
    {
        public override string Description =>
            "Cheap and quickly made cigar. Stimulant.";

        public override float HealthRestore => -3;

        public override double MedicalToxicity => 0.10;

        public override string Name => "Cheap cigar";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHigh>(intensity: 0.30); // adds high effect
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalSmoke;
        }
    }
}