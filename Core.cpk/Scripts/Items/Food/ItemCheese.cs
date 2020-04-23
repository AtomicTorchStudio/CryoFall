namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemCheese : ProtoItemFood
    {
        public override string Description => "Traditional food made from milk by coagulation of the milk protein.";

        public override float FoodRestore => 8;

        public override TimeSpan FreshnessDuration => ExpirationDuration.LongLasting;

        public override string Name => "Cheese";

        public override ushort OrganicValue => 5;

        protected override void ServerOnEat(ItemEatData data)
        {
            data.Character.ServerAddStatusEffect<StatusEffectSavoryFood>(intensity: 0.1);

            base.ServerOnEat(data);
        }
    }
}