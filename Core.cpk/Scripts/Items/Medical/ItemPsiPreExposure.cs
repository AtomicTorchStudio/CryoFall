namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;

    public class ItemPsiPreExposure : ProtoItemMedical
    {
        public override string Description =>
            "Special neuro-modulating chemical that can help mitigate neural damage from psi field exposure.";

        public override double MedicalToxicity => 0.15;

        public override string Name => "Psi blocker";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectProtectionPsi>(intensity: 0.50); // add psi protection
        }
    }
}