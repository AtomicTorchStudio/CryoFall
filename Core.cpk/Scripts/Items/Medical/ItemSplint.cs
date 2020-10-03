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

        public override double CooldownDuration => MedicineCooldownDuration.Medium;

        public override string Description => "Splint is used to set a broken bone and allow it to heal.";

        public override double MedicalToxicity => 0;

        public override string Name => "Splint";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                // adds splinted leg status effect when player has a broken leg status effect
                .WillAddEffect<StatusEffectSplintedLeg>(
                    condition: context => context.Character.SharedHasStatusEffect<StatusEffectBrokenLeg>(),
                    isHidden: true)
                // and remove broken leg
                .WillRemoveEffect<StatusEffectBrokenLeg>();
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalBandage;
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