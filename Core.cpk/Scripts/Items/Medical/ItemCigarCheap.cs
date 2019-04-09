namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemCigarCheap : ProtoItemMedical
    {
        public override string Description =>
            "Cheap and quickly made cigar. Stimulant.";

        public override float HealthRestore => -3;

        public override double MedicalToxicity => 0.10;

        public override string Name => "Cheap cigar";

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalSmoke;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // adds high effect
            character.ServerAddStatusEffect<StatusEffectHigh>(intensity: 0.3);

            base.ServerOnUse(character, currentStats);
        }
    }
}