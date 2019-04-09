namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemHeatPreExposure : ProtoItemMedical
    {
        public override string Description =>
            "Thermal-resistant gel prevents direct thermal transfer and reflects radiant heat, allowing you to enter high-temperature areas for a short duration.";

        public override double MedicalToxicity => 0.05;

        public override string Name => "Heat-resistant gel";

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            //add heat protection
            character.ServerAddStatusEffect<StatusEffectProtectionHeat>(intensity: 0.5); // 5 minutes

            base.ServerOnUse(character, currentStats);
        }
    }
}