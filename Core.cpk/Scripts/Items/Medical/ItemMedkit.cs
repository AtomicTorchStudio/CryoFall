namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemMedkit : ProtoItemMedical
    {
        public override double CooldownDuration => MedicineCooldownDuration.Long;

        public override string Description =>
            "All-in-one medical kit. Restores health over time, removes nausea and bleeding, even contains a splint for broken bones.";

        public override double MedicalToxicity => 0.24;

        public override string Name => "MedKit";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHealingSlow>(intensity: 0.70) // 70 seconds (70hp)
                .WillAddEffect<StatusEffectHealingFast>(intensity: 0.10) // 1 seconds (+10 HP each second)
                .WillRemoveEffect<StatusEffectBleeding>()
                .WillRemoveEffect<StatusEffectNausea>()
                // adds splinted leg status effect when player has a broken leg status effect
                .WillAddEffect<StatusEffectSplintedLeg>(
                    condition: context => context.Character.SharedHasStatusEffect<StatusEffectBrokenLeg>(),
                    isHidden: true)
                // and remove broken leg
                .WillRemoveEffect<StatusEffectBrokenLeg>();
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalMedkit;
        }
    }
}