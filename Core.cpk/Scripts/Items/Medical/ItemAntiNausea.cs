namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemAntiNausea : ProtoItemMedical
    {
        public const string NotificationNoNausea_Message =
            "You don't have nausea. There's no point in using this medicine now.";

        public const string NotificationNoNausea_Title = "No nausea";

        public override string Description =>
            "This medicine helps to completely remove the symptoms of nausea.";

        public override double MedicalToxicity => 0.05;

        public override string Name => "Anti-nausea";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillRemoveEffect<StatusEffectNausea>(); // removes nausea
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemMedicalTablets;
        }

        protected override bool SharedCanUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // does the player even have nausea?
            if (character.SharedHasStatusEffect<StatusEffectNausea>())
            {
                return true;
            }

            if (IsClient)
            {
                NotificationSystem.ClientShowNotification(
                    NotificationNoNausea_Title,
                    NotificationNoNausea_Message,
                    NotificationColor.Bad,
                    icon: this.Icon);
            }

            return false;
        }
    }
}