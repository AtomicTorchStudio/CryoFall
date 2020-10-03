namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;

    public class ItemHeatPreExposure : ProtoItemMedical
    {
        public override double CooldownDuration => MedicineCooldownDuration.Medium;

        public override string Description =>
            "Thermal-resistant gel prevents direct thermal transfer and reflects radiant heat, allowing you to enter high-temperature areas for a short duration.";

        public override double MedicalToxicity => 0.05;

        public override string Name => "Heat-resistant gel";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectProtectionHeat>(intensity: 0.50); //add heat protection
        }
    }
}