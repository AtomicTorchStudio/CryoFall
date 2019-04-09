namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.FoodSpoilageSystem;

    public class ItemCoffeeCup : ProtoItemFood
    {
        public override string Description => "Cup of hot espresso. Helps you do stupid things, now with more energy.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Cup of coffee";

        public override ushort OrganicValue => 2;

        public override float WaterRestore
            => 5; // not that much water restore because technically coffee is a diuretic.

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrink;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            double energyRushIntensity;

            switch (data.Freshness)
            {
                case FoodFreshness.Green:
                    energyRushIntensity = 0.2; // 2 minutes
                    break;

                case FoodFreshness.Yellow:
                    energyRushIntensity = 0.1; // 1 minute
                    break;

                case FoodFreshness.Red:
                default:
                    energyRushIntensity = 0; // no effect (spoiled coffee!)
                    break;
            }

            if (energyRushIntensity > 0)
            {
                data.Character.ServerAddStatusEffect<StatusEffectEnergyRush>(energyRushIntensity);
            }

            base.ServerOnEat(data);
        }
    }
}