namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class StatusEffectMedicineOveruse : ProtoStatusEffect
    {
        public const string NotificationMessage =
            "You overused medical items and suffered health damage as a result.";

        public override string Description =>
            "You've been using too much medicine in a short period of time. Give it some rest, or suffer negative consequences to your health. Ignoring this warning could be deadly.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Medicine overuse";

        public override double VisibilityIntensityThreshold => 0.5; //becomes visible when reaches 50%

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            // modify intensity change based on artificial liver or any other thing that modifies it
            intensityToAdd *= data.Character.SharedGetFinalStatMultiplier(StatName.MedicineToxicityMultiplier);
            if (intensityToAdd <= 0)
            {
                return;
            }

            if (data.Intensity > 0.5)
            {
                // need to apply damage to health as the medicine overuse is severe
                // calculate damage, essentially current intensity + added intensity as percent, and then divided by 5
                var damage = (data.Intensity + intensityToAdd) * 100 / 5;

                // reduce character health
                var stats = data.CharacterCurrentStats;
                stats.ServerReduceHealth(damage, data.StatusEffect);

                // notify the character why the health was reduced
                this.CallClient(data.Character,
                                _ => _.ClientRemote_DisplayOveruseDamageNotification());
            }

            // increase the status effect intensity
            data.Intensity += intensityToAdd;
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_DisplayOveruseDamageNotification()
        {
            NotificationSystem.ClientShowNotification(
                this.Name,
                NotificationMessage,
                NotificationColor.Bad,
                this.Icon);
        }
    }
}