namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectProtectionHeat : ProtoStatusEffect
    {
        public override string Description =>
            "You are covered in a heat-resistant gel, significantly reducing any thermal damage you take from environmental sources.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Heat protection";

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.HeatEffectMultiplier, -50);
        }
    }
}