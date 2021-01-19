namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemCigarettes : ProtoItemMedical
    {
        public override double CooldownDuration => MedicineCooldownDuration.None;

        public override string Description =>
            "Off-world cigarettes brand.";

        public override float HealthRestore => -3;

        public override double MedicalToxicity => 0.05;

        public override string Name => "Cigarettes";

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