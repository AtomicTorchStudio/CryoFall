namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemBottleWaterStale : ProtoItemFood
    {
        public override string Description =>
            "Stale water drawn from a lake. Should be boiled first to make it safe. Though, it could still hydrate you in a pinch.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public override bool IsAvailableInCompletionist => false;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Bottle with stale water";

        public override ushort OrganicValue => 0;

        public override float WaterRestore => 30;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                // adds food poisoning unless you have artificial stomach
                .WillAddEffect<StatusEffectNausea>(intensity: 0.50,
                                                   condition: context => !context.Character.SharedHasPerk(
                                                                             StatName.PerkEatSpoiledFood));
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrink;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            ItemBottleEmpty.ServerSpawnEmptyBottle(data.Character);
            base.ServerOnEat(data);
        }
    }
}