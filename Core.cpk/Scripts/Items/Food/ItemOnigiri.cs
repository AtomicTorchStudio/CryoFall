namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemOnigiri : ProtoItemFood
    {
        public override string Description =>
            "Also known as rice balls. Can be conveniently eaten on the go. Simple and filling. Spices keep it fresh for longer.";

        public override float FoodRestore => 12;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override string Name => "Onigiri";

        public override ushort OrganicValue => 5;

        public override float StaminaRestore => 25;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectSavoryFood>(intensity: 0.15);
        }
    }
}