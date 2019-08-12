using AtomicTorch.CBND.CoreMod.Stats;

namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    public class StatusEffectProtectionToxins : ProtoStatusEffect
    {
        public override string Description =>
            "You are currently under the effect of toxin protection and any new toxin exposure has less of an effect on you.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Toxin protection";

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.ToxinsIncreaseRateMultiplier, -50);
        }
    }
}