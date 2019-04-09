namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemCigarPremium : ProtoItemMedical
    {
        public override string Description =>
            "Fine cigar with exquisite aroma and flavor. Stimulant.";

        public override float HealthRestore => -1;

        public override double MedicalToxicity => 0.05;

        public override string Name => "Premium cigar";

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalSmoke;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // adds high effect
            character.ServerAddStatusEffect<StatusEffectHigh>(intensity: 0.7);

            base.ServerOnUse(character, currentStats);
        }
    }
}