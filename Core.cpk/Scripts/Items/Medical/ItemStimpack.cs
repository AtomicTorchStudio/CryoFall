namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemStimpack : ProtoItemMedical
    {
        public override string Description =>
            "Stimpack is used to restore a moderate amount of health almost instantly. Especially useful in combat, but should not be overused as that can lead to serious complications.";

        public override double MedicalToxicity => 0.45;

        public override string Name => "Stimpack";

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalStimpack;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // adds fast health regeneration
            character.ServerAddStatusEffect<StatusEffectHealingFast>(intensity: 0.4); // 4 seconds (+10 HP each second)

            base.ServerOnUse(character, currentStats);
        }
    }
}