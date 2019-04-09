namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemSplint : ProtoItemMedical
    {
        public const string NotificationNoBrokenBones_Message =
            "You don't have any broken bones. There's no point in using a splint now.";

        public const string NotificationNoBrokenBones_Title = "No broken bones";

        public override string Description => "Splint is used to set a broken bone and allow it to heal.";

        public override double MedicalToxicity => 0;

        public override string Name => "Splint";

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalBandage;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // remove broken leg
            character.ServerRemoveStatusEffectIntensity<StatusEffectBrokenLeg>(intensityToRemove: 1);

            //add splinted leg
            character.ServerAddStatusEffect<StatusEffectSplintedLeg>(intensity: 1);

            base.ServerOnUse(character, currentStats);
        }

        protected override bool SharedCanUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // does the player even have broken leg?
            if (character.SharedHasStatusEffect<StatusEffectBrokenLeg>())
            {
                return true;
            }

            if (IsClient)
            {
                NotificationSystem.ClientShowNotification(
                    NotificationNoBrokenBones_Title,
                    NotificationNoBrokenBones_Message,
                    NotificationColor.Bad,
                    icon: this.Icon);
            }

            return false;
        }
    }
}