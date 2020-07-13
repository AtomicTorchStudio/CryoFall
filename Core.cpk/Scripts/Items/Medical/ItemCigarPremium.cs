namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCigarPremium : ProtoItemMedical
    {
        public override string Description =>
            "Fine cigar with exquisite aroma and flavor. Stimulant.";

        public override float HealthRestore => -1;

        public override double MedicalToxicity => 0.05;

        public override string Name => "Premium cigar";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHigh>(intensity: 0.70); // adds high effect
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalSmoke;
        }
    }
}