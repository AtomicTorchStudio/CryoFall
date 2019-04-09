namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemCigarNormal : ProtoItemMedical
    {
        public override string Description =>
            "Decent cigar with full body flavor. Stimulant.";

        public override float HealthRestore => -2;

        public override double MedicalToxicity => 0.07;

        public override string Name => "Cigar";

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalSmoke;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // adds high effect
            character.ServerAddStatusEffect<StatusEffectHigh>(intensity: 0.5);

            base.ServerOnUse(character, currentStats);
        }
    }
}