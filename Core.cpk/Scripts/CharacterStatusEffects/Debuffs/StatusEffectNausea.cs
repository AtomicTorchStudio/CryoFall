namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class StatusEffectNausea : ProtoStatusEffect
    {
        public const string NotificationNauseousMessage = "Cannot eat or drink.";

        public override string Description =>
            "You don't feel too well... You cannot eat or drink anything while you are nauseated. Give it some time or take anti-nausea medication.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Nausea";

        /// <summary>
        /// Shared method for checking (and notifying) whether the character is nauseous.
        /// </summary>
        public static bool SharedCheckIsNauseous(
            ICharacter character,
            bool showNotificationIfNauseous = true)
        {
            // if the character doesn't have nausea then it's all good
            if (!character.SharedHasStatusEffect<StatusEffectNausea>())
            {
                return false;
            }

            // cannot eat/drink while nausea
            if (showNotificationIfNauseous && IsClient)
            {
                NotificationSystem.ClientShowNotification(
                    GetProtoEntity<StatusEffectNausea>().Name,
                    NotificationNauseousMessage,
                    NotificationColor.Bad,
                    icon: GetProtoEntity<StatusEffectNausea>().Icon);
            }

            return true;
        }

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectNauseaManager.Refresh();
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectNauseaManager.Refresh();
        }
    }
}