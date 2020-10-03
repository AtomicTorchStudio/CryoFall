namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemCarbonara : ProtoItemFood
    {
        public override string Description => "Pasta, egg, cheese and meat combine into this delicious goodness!";

        public override float FoodRestore => 40;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Carbonara";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 50;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHeartyFood>(intensity: 0.50)
                .WillAddEffect<StatusEffectSavoryFood>(intensity: 0.10);
        }
    }
}