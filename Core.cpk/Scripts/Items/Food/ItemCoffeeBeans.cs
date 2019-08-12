namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;

    public class ItemCoffeeBeans : ProtoItemFood
    {
        public const string NotificationEatingCoffeeBadIdea =
            "Maybe eating raw coffee beans wasn't the brightest idea...";

        public override string Description =>
            "Can be used to prepare one of the most widespread, traditional drinks in the galaxy.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.LongLasting;

        public override string Name => "Coffee beans";

        public override ushort OrganicValue => 2;

        public override float WaterRestore => -3;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodCrunchy;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            var character = data.Character;

            // 100% chance to get food poisoning unless you have artificial stomach
            if (!character.SharedHasPerk(StatName.PerkEatSpoiledFood))
            {
                character.ServerAddStatusEffect<StatusEffectNausea>(intensity: 0.2); // 2 minute
            }

            base.ServerOnEat(data);
        }

        protected override bool SharedCanEat(ItemEatData data)
        {
            if (!base.SharedCanEat(data))
            {
                return false;
            }

            if (IsClient
                && !data.Character.SharedHasPerk(StatName.PerkEatSpoiledFood))
            {
                NotificationSystem.ClientShowNotification(
                    NotificationEatingCoffeeBadIdea,
                    color: NotificationColor.Bad,
                    icon: this.Icon);
            }

            return true;
        }
    }
}