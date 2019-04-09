namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemMedkit : ProtoItemMedical
    {
        public override string Description =>
            "All-in-one medical kit. Restores health over time, removes nausea and bleeding, even contains a splint for broken bones.";

        public override double MedicalToxicity => 0.24;

        public override string Name => "MedKit";

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalMedkit;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // adds health regeneration
            character.ServerAddStatusEffect<StatusEffectHealingSlow>(intensity: 0.75); // 75 seconds (75hp)

            // removes bleeding
            character.ServerRemoveStatusEffectIntensity<StatusEffectBleeding>(intensityToRemove: 1);

            // removes nausea
            character.ServerRemoveStatusEffectIntensity<StatusEffectNausea>(intensityToRemove: 1);

            // checks if the player has a broken leg and if so - fixes it too
            if (character.SharedHasStatusEffect<StatusEffectBrokenLeg>())
            {
                character.ServerRemoveStatusEffectIntensity<StatusEffectBrokenLeg>(intensityToRemove: 1);
                character.ServerAddStatusEffect<StatusEffectSplintedLeg>(intensity: 1);
            }

            base.ServerOnUse(character, currentStats);
        }
    }
}