namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemHemostatic : ProtoItemMedical
    {
        public const string NotificationNoBleeding_Message =
            "You aren't bleeding. There's no point in using hemostatic now.";

        public const string NotificationNoBleeding_Title = "No bleeding";

        public override string Description =>
            "Hemostatic can be used to completely stop bleeding and partially reduce any further bleeding for the duration of its effect.";

        public override double MedicalToxicity => 0.15;

        public override string Name => "Hemostatic";

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            // remove all bleeding
            character.ServerRemoveStatusEffectIntensity<StatusEffectBleeding>(intensityToRemove: 1);

            // add bleeding protection
            character.ServerAddStatusEffect<StatusEffectProtectionBleeding>(intensity: 0.5); // 5 minutes

            base.ServerOnUse(character, currentStats);
        }

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
                    NotificationNoBleeding_Title,
                    NotificationNoBleeding_Message,
                    NotificationColor.Bad,
                    icon: this.Icon);
            }

            return false;
        }
    }
}