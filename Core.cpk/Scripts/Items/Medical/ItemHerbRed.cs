namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemHerbRed : ProtoItemMedical
    {
        public override string Description =>
            "Medicinal herb with a strong stimulating effect. Should not be taken raw, as it contains toxins.";

        public override double MedicalToxicity => 0.25;

        public override string Name => "Red herb";

        public override float StaminaRestore => 50;

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            character.ServerAddStatusEffect<StatusEffectEnergyRush>(intensity: 0.05); // 30 seconds
            character.ServerAddStatusEffect<StatusEffectToxins>(intensity: 0.1); // 30 seconds

            base.ServerOnUse(character, currentStats);
        }
    }
}