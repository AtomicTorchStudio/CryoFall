namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral.Client;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    /// <summary>
    /// Note: drunk status effect affects psi protection and offers 30% defense (calculated inside the psi effect)
    /// Note: it also works the same as painkiller but reduces pain by half (calculated inside the pain effect)
    /// Note: it also provides 100% protection against dazed effect (calculated inside the dazed effect)
    /// </summary>
    public class StatusEffectDrunk : ProtoStatusEffect
    {
        public const string NotificationMessage =
            "You've been drinking like a fish. No wonder you want to shout at your shoes now...";

        public override string Description
            => "You've been drinking...";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Drunk";

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectNauseaManager.Refresh();
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectNauseaManager.Refresh();
        }

        protected override void PrepareEffects(Effects effects)
        {
            // energy regeneration +25%
            effects.AddPercent(this, StatName.StaminaRegenerationPerSecond, 25);

            // no health regeneration while under the effect of alcohol
            effects.AddPercent(this, StatName.HealthRegenerationPerSecond, -100);

            // movement speed -15%
            effects.AddPercent(this, StatName.MoveSpeed, -15);
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            // is the player hammered?
            if (data.Intensity + intensityToAdd > 0.99)
            {
                data.Character.ServerAddStatusEffect<StatusEffectNausea>(0.5);

                // send notification that the player has nausea now
                this.CallClient(data.Character, _ => _.ClientRemote_ShowDrunkNotification());
            }

            // otherwise add as normal
            base.ServerAddIntensity(data, intensityToAdd);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_ShowDrunkNotification()
        {
            NotificationSystem.ClientShowNotification(
                this.Name,
                NotificationMessage,
                NotificationColor.Bad,
                icon: this.Icon);
        }
    }
}