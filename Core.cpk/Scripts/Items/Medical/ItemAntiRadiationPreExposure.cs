namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemAntiRadiationPreExposure : ProtoItemMedical
    {
        public override string Description =>
            "This pre-exposure radiation aid could be used to prevent accumulation of radionuclides in the body and mitigate effects of ionizing radiation.";

        public override double MedicalToxicity => 0.2;

        public override string Name => "Radiation prevention aid";

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // remove radiation
            character.ServerRemoveStatusEffectIntensity<StatusEffectRadiationPoisoning>(intensityToRemove: 0.1);

            // add radiation protection
            character.ServerAddStatusEffect<StatusEffectProtectionRadiation>(intensity: 0.5); // 5 minutes

            base.ServerOnUse(character, currentStats);
        }
    }
}