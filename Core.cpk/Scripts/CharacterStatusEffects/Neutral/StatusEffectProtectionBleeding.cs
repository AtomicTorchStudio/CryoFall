using AtomicTorch.CBND.CoreMod.Stats;

namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    public class StatusEffectProtectionBleeding : ProtoStatusEffect
    {
        public override string Description =>
            "You are currently under the effect of hemostatic medicine, which reduces the severity of any new bleeding wounds received.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Bleeding protection";

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.BleedingIncreaseRateMultiplier, -50);
        }
    }
}