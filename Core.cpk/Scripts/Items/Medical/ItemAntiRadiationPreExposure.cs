namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;

    public class ItemAntiRadiationPreExposure : ProtoItemMedical
    {
        public override string Description =>
            "This pre-exposure radiation aid could be used to prevent accumulation of radionuclides in the body and mitigate effects of ionizing radiation.";

        public override double MedicalToxicity => 0.2;

        public override string Name => "Radiation prevention aid";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillRemoveEffect<StatusEffectRadiationPoisoning
                    >(intensityToRemove: 0.10)                                    // remove accumulated radiation
                .WillAddEffect<StatusEffectProtectionRadiation>(intensity: 0.50); // add radiation protection
        }
    }
}