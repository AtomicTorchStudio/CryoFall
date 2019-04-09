namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemTequila : ProtoItemFood
    {
        public override string Description =>
            "Beautiful liquor distilled from plants found in the desert.";

        public override float FoodRestore => 5; // Yes, alcohol has calories

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Tequila";

        public override ushort OrganicValue => 0;

        public override float WaterRestore => -5; // dehydrates

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrinkAlcohol;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            // 4 minutes
            data.Character.ServerAddStatusEffect<StatusEffectDrunk>(intensity: 0.4);

            base.ServerOnEat(data);
        }
    }
}