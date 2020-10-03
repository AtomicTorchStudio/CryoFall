namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemSandwich : ProtoItemFood
    {
        public override string Description =>
            "Soft bread, juicy meat and veggies together make this the absolute classic!";

        public override float FoodRestore => 20;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Sandwich";

        public override ushort OrganicValue => 5;

        public override float StaminaRestore => 25;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectSavoryFood>(intensity: 0.20);
        }
    }
}