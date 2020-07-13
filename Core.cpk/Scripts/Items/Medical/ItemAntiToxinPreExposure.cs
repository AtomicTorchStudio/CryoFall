namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;

    public class ItemAntiToxinPreExposure : ProtoItemMedical
    {
        public override string Description =>
            "This pre-exposure toxin aid could be used to prevent absorption of any toxins by the body, as well as boost the immune system to mitigate the effects of any toxins that do enter the bloodstream.";

        public override double MedicalToxicity => 0.2;

        public override string Name => "Toxin prevention aid";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillRemoveEffect<StatusEffectToxins>(intensityToRemove: 0.10) // remove toxins
                .WillAddEffect<StatusEffectProtectionToxins>(intensity: 0.50); // add toxin protection
        }
    }
}