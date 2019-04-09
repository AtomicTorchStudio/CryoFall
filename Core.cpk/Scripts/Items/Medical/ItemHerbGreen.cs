namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemHerbGreen : ProtoItemMedical
    {
        public override string Description =>
            "Medicinal herb with unique healing and antibacterial properties. Can be used as-is, but becomes much more potent once properly prepared.";

        public override double MedicalToxicity => 0.05;

        public override string Name => "Green herb";

        public override float StaminaRestore => 15;

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            character.ServerAddStatusEffect<StatusEffectHealingSlow>(intensity: 0.07); // 7 seconds (7hp)

            base.ServerOnUse(character, currentStats);
        }
    }
}