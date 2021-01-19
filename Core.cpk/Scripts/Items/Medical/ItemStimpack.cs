namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemStimpack : ProtoItemMedical
    {
        public override double CooldownDuration => MedicineCooldownDuration.Medium;

        public override string Description =>
            "Stimpack is used to restore a moderate amount of health almost instantly. Especially useful in combat, but should not be overused as that can lead to serious complications.";

        public override double MedicalToxicity => 0.45;

        public override string Name => "Stimpack";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHealingFast>(intensity: 0.40); // adds fast health regeneration
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalStimpack;
        }
    }
}