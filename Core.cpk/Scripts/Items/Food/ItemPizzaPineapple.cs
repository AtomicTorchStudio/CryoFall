namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemPizzaPineapple : ProtoItemFood
    {
        public override string Description => "The holy war still rages to this day...";

        public override float FoodRestore => 30;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Pineapple pizza";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 50;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHeartyFood>(intensity: 0.40);
        }
    }
}