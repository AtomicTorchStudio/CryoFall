namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCigarNormal : ProtoItemMedical
    {
        public override double CooldownDuration => MedicineCooldownDuration.None;

        public override string Description =>
            "Decent cigar with full body flavor. Stimulant.";

        public override float HealthRestore => -2;

        public override double MedicalToxicity => 0.07;

        public override string Name => "Cigar";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHigh>(intensity: 0.50); // adds high effect
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalSmoke;
        }
    }
}