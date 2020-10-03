namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemSushi : ProtoItemFood
    {
        public override string Description =>
            "Rice topped with raw fish and spices. Simple, but unforgettable flavor.";

        public override float FoodRestore => 15;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override string Name => "Sushi";

        public override ushort OrganicValue => 10;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectSavoryFood>(intensity: 0.20);
        }
    }
}