namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using System;

    public class ItemSaladVegetable : ProtoItemFood, IProtoItemOrganic
    {
        public override string Description =>
            "Mixed vegetable salad. Very healthy and surprisingly filling!";

        public override float FoodRestore => 25;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Vegetable salad";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 25;

        public override float WaterRestore => 10;

        protected override void ServerOnEat(ItemEatData data)
        {
            data.Character.ServerAddStatusEffect<StatusEffectHealthyFood>(intensity: 0.3);

            base.ServerOnEat(data);
        }
    }
}