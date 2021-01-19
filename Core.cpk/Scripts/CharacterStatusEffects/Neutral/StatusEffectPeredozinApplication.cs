namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class StatusEffectPeredozinApplication : ProtoStatusEffect
    {
        public const string Notification_CannotApply_Message = "Please wait until peredozin application is finished.";

        public override string Description => GetProtoEntity<ItemPeredozin>().Description;

        public override StatusEffectDisplayMode DisplayMode
            => StatusEffectDisplayMode.IconShowTimeRemains
               | StatusEffectDisplayMode.TooltipShowTimeRemains;

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / ItemPeredozin.MedicalCooldownDuration;

        public override bool IsRemovedOnRespawn => false;

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => GetProtoEntity<ItemPeredozin>().Name;

        public override double ServerUpdateIntervalSeconds => 0.1;

        public static bool SharedCheckCanEquipArmor(ICharacter character, bool clientShowNotification)
        {
            if (!character.SharedHasStatusEffect<StatusEffectPeredozinApplication>())
            {
                return true;
            }

            if (IsClient
                && clientShowNotification)
            {
                NotificationSystem.ClientShowNotification(
                    ItemsContainerCharacterEquipment.NotificationCannotEquip,
                    Notification_CannotApply_Message,
                    NotificationColor.Bad,
                    Api.GetProtoEntity<StatusEffectPeredozinApplication>().Icon);
            }

            return false;
        }
    }
}