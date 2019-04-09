namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;

    public class ItemChiliPepper : ProtoItemFood
    {
        public const string NotificationSpicy = "Ouch! That is spicy!";

        public override string Description =>
            "A popular addition to many dishes that gives them more spiciness and flavor.";

        public override float FoodRestore => 2;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Chili pepper";

        public override ushort OrganicValue => 3;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodFruit;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            // 100% chance to get pain effect
            data.Character.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.30); // 30 seconds

            base.ServerOnEat(data);
        }

        protected override bool SharedCanEat(ItemEatData data)
        {
            if (!base.SharedCanEat(data))
            {
                return false;
            }

            if (IsClient)
            {
                NotificationSystem.ClientShowNotification(
                    NotificationSpicy,
                    color: NotificationColor.Bad,
                    icon: this.Icon);
            }

            return true;
        }
    }
}