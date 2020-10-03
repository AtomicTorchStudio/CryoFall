namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemStrengthBoostBig : ProtoItemMedical
    {
        public override double CooldownDuration => MedicineCooldownDuration.None;

        public override string Description =>
            "Strength-boost medicine makes you temporarily stronger, increasing performance in physical tasks.";

        public override double MedicalToxicity => 0.3;

        public override string Name => "Large strength boost";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectStrength>(intensity: 0.70); // adds strength boost
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrink;
        }
    }
}