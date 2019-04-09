namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBeer : ProtoItemFood
    {
        public override string Description =>
            "Refreshing and cool. This light beer is brewed from high-quality grains.";

        public override float FoodRestore => 3; // Yes, beer has calories

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        // Restores 1 hp, but given that you cannot drink more than a couple of bottles, it cannot be abused.
        public override float HealthRestore => 1;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Beer";

        public override ushort OrganicValue => 0;

        public override float WaterRestore => 3; // doesn't hydrate much, because of alcohol

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrinkAlcohol;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            // 2 minutes (so you need more than 5 bottles to be hammered)
            data.Character.ServerAddStatusEffect<StatusEffectDrunk>(intensity: 0.2);

            base.ServerOnEat(data);
        }
    }
}