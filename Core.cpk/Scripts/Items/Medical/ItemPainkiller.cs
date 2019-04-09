namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemPainkiller : ProtoItemMedical
    {
        public override string Description =>
            "Painkiller can be used to completely remove and prevent any pain for the duration of its effect.";

        public override double MedicalToxicity => 0.1;

        public override string Name => "Painkiller";

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalTablets;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // remove all pain
            character.ServerRemoveStatusEffectIntensity<StatusEffectPain>(intensityToRemove: 1);

            // add pain blocking
            character.ServerAddStatusEffect<StatusEffectProtectionPain>(intensity: 0.5); // 5 minutes

            base.ServerOnUse(character, currentStats);
        }
    }
}