namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class ItemSaladFruit : ProtoItemFood, IProtoItemOrganic
    {
        public override string Description =>
            "Mixed fruit salad. Sweet, healthy and filling!";

        public override float FoodRestore => 20;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Fruit salad";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 25;

        public override float WaterRestore => 5;

        protected override void ServerOnEat(ItemEatData data)
        {
            data.Character.ServerAddStatusEffect<StatusEffectHealthyFood>(intensity: 0.25);

            base.ServerOnEat(data);
        }
    }
}