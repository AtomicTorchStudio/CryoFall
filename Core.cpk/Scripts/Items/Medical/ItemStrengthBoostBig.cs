namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemStrengthBoostBig : ProtoItemMedical
    {
        public override string Description =>
            "Strength-boost medicine makes you temporarily stronger, increasing performance in physical tasks.";

        public override double MedicalToxicity => 0.3;

        public override string Name => "Strength boost (big)";

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrink;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            character.ServerAddStatusEffect<StatusEffectStrength>(intensity: 0.7); // 7 minutes

            base.ServerOnUse(character, currentStats);
        }
    }
}