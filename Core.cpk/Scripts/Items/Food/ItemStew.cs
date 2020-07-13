namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemStew : ProtoItemFood
    {
        public override string Description => "Tasty meat stew prepared with mushrooms.";

        public override float FoodRestore => 40;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override float HealthRestore => 3;

        public override string Name => "Meat and mushrooms stew";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 50;

        public override float WaterRestore => 5;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHeartyFood>(intensity: 0.60);
        }
    }
}