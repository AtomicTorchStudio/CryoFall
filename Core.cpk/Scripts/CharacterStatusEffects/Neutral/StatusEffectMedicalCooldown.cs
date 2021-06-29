namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class StatusEffectMedicalCooldown : ProtoStatusEffect
    {
        public const string Notification_UnderCooldown_Message
            = "Please wait until the cooldown period has ended.";

        private static readonly Lazy<StatusEffectMedicalCooldown> LazyInstance
            = new(Api.GetProtoEntity<StatusEffectMedicalCooldown>);

        public static double MaxDuration => MedicineCooldownDuration.Maximum;

        public override string Description =>
            "You've recently used a medical item. You cannot use any more until the end of the cooldown period.";

        public override StatusEffectDisplayMode DisplayMode
            => StatusEffectDisplayMode.IconShowTimeRemains
               | StatusEffectDisplayMode.TooltipShowTimeRemains;

        public override double IntensityAutoDecreasePerSecondValue
            => 1 / MaxDuration;

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Medical cooldown";

        public override double ServerUpdateIntervalSeconds => 0.1;

        public static void ClientShowCooldownNotification()
        {
            var proto = LazyInstance.Value;

            NotificationSystem.ClientShowNotification(
                proto.Name,
                message: "[*]"
                         + StatName.PerkCannotAttack.GetDescription()
                         + "[*]"
                         + StatName.PerkCannotUseMedicalItems.GetDescription()
                         + "[br]"
                         + "[br]"
                         + Notification_UnderCooldown_Message,
                color: NotificationColor.Bad,
                icon: proto.Icon);
        }

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPerk(this, StatName.PerkCannotAttack);
            effects.AddPerk(this, StatName.PerkCannotUseMedicalItems);
        }
    }
}