namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemDrinkHerbal : ProtoItemFood
    {
        public override string Description =>
            "Popular drink made from herbal ingredients. Supposedly good for your health and especially for detoxifying the body.";

        public override float FoodRestore => 3;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Preserved;

        public override float HealthRestore => 1;

        public override string ItemUseCaption => ItemUseCaptions.Drink;

        public override string Name => "Herbal drink";

        public override ushort OrganicValue => 0;

        public override float StaminaRestore => 30;

        public override float WaterRestore => 30;

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemFoodDrinkCan;
        }

        protected override void ServerOnEat(ItemEatData data)
        {
            data.Character.ServerRemoveStatusEffectIntensity<StatusEffectToxins>(0.05); // removes 15 sec of toxins

            base.ServerOnEat(data);
        }
    }
}