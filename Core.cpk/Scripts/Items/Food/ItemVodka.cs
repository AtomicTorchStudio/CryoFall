namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemVodka : ProtoItemFood
    {
        public override string Description =>
            "The fiery liquid of gods. Abuse leads to consequences, so don't drink too much. It also dehydrates you. And no, it doesn't help against radiation.";

        // Yes! Vodka is high in calories in real life, in fact 40% vodka is 75 kcal per 100 ml!
        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.LongLasting;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Vodka";

        public override ushort OrganicValue => 0;

        public override float WaterRestore => -5; // dehydrates

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrinkAlcohol;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            // 4.5 minutes (so 2 vodka bottles will be almost 10 minutes, so after the third bottle you will be vomiting)
            data.Character.ServerAddStatusEffect<StatusEffectDrunk>(intensity: 0.45);

            base.ServerOnEat(data);
        }
    }
}