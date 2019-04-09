namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemBandage : ProtoItemMedical
    {
        public override string Description =>
            "Bandages can be useful to stop bleeding and help heal the wound.";

        public override double MedicalToxicity => 0;

        public override string Name => "Bandage";

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalBandage;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // remove some bleeding
            character.ServerRemoveStatusEffectIntensity<StatusEffectBleeding>(intensityToRemove: 0.3); // 3 minutes

            // add small healing effect
            character.ServerAddStatusEffect<StatusEffectHealingSlow>(intensity: 0.10); // 10 seconds (10hp)

            base.ServerOnUse(character, currentStats);
        }

        /*
        Not needed now, as the bandage also heals now in addition to just removing the bleeding
        protected override bool SharedCanUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // does the player even have bleeding?
            if (character.SharedHasStatusEffect<StatusEffectBleeding>())
            {
                return true;
            }

            if (IsClient)
            {
                NotificationSystem.ClientShowNotification(
                    "No bleeding",
                    "You don't have a bleeding. There's no point in using a bandage now.",
                    NotificationColor.Bad,
                    icon: this.Icon);
            }

            return false;
        }*/
    }
}