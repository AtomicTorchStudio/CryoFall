namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.ItemFreshnessSystem;

    public class ItemCoffeeCup : ProtoItemFood
    {
        public override string Description => "Cup of hot espresso. Helps you do stupid things, now with more energy.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Cup of coffee";

        public override ushort OrganicValue => 2;

        public override float WaterRestore => 10;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrink;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            data.Character.ServerAddStatusEffect<StatusEffectEnergyRush>(intensity: 0.2); // minutes

            base.ServerOnEat(data);
        }
    }
}