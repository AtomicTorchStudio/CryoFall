namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemChiliBeans : ProtoItemFood
    {
        public override string Description => "The ultimate bachelor food.";

        public override float FoodRestore => 25;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Hot chili beans";

        public override ushort OrganicValue => 0;

        public override float StaminaRestore => 100;

        protected override void ServerOnEat(ItemEatData data)
        {
            data.Character.ServerAddStatusEffect<StatusEffectWellFed>(intensity: 0.3);

            base.ServerOnEat(data);
        }
    }
}