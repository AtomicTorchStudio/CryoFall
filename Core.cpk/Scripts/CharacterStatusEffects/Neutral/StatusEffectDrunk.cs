namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral.Client;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

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

            // movement speed -5%
            effects.AddPercent(this, StatName.MoveSpeed, -5);

            // pain -50%
            effects.AddPercent(this, StatName.PainIncreaseRateMultiplier, -50);

            // psi damage -25%
            effects.AddPercent(this, StatName.PsiEffectMultiplier, -25);

            // dazed -100%, dazed effect cannot be added if character is drunk
            effects.AddPercent(this, StatName.DazedIncreaseRateMultiplier, -100);

            // +5 fishing knowledge :-)
            effects.AddValue(this, StatName.FishingKnowledgeLevel, 5);
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