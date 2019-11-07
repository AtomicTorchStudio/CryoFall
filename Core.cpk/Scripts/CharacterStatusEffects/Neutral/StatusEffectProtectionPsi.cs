namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectProtectionPsi : ProtoStatusEffect
    {
        public override string Description =>
            "You are currently under the effect of psi protection and any psi exposure has less of an effect on you.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Psi protection";

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.PsiEffectMultiplier, -50);
        }
    }
}