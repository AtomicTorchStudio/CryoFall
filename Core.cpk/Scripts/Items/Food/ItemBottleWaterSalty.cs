namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBottleWaterSalty : ProtoItemFood
    {
        public override string Description =>
            "Salty sea water. Rather than hydrate you, it will only make you thirstier. Could be evaporated to extract salt.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Unlimited;

        public override bool IsAvailableInCompletionist => false;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public override string Name => "Bottle with salt water";

        public override ushort OrganicValue => 0;

        public override float WaterRestore => -10;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectNausea>(intensity: 0.50); // adds food poisoning
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