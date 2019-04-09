namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemPsiPreExposure : ProtoItemMedical
    {
        public override string Description =>
            "Special neuro-modulating chemical that can help mitigate neural damage from psi field exposure.";

        public override double MedicalToxicity => 0.15;

        public override string Name => "Psi blocker";

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // add psi protection
            character.ServerAddStatusEffect<StatusEffectProtectionPsi>(intensity: 0.5); // 5 minutes

            base.ServerOnUse(character, currentStats);
        }
    }
}