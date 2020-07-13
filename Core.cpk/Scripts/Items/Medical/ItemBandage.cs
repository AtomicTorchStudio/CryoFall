namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBandage : ProtoItemMedical
    {
        public override string Description =>
            "Bandages can be useful to stop bleeding and help heal the wound.";

        public override double MedicalToxicity => 0;

        public override string Name => "Bandage";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillRemoveEffect<StatusEffectBleeding>(intensityToRemove: 0.30) // remove some bleeding
                .WillAddEffect<StatusEffectHealingSlow>(intensity: 0.10);        // add small healing effect
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalBandage;
        }
    }
}