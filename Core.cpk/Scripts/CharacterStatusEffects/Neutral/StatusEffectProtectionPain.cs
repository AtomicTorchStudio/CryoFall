using AtomicTorch.CBND.CoreMod.Stats;

namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    public class StatusEffectProtectionPain : ProtoStatusEffect
    {
        public override string Description =>
            "You are currently under the effect of strong painkillers. You don't feel any pain, whether you like it or not.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Pain protection";

        protected override void PrepareEffects(Effects effects)
        {
            // complete defense from pain
            effects.AddPercent(this, StatName.PainIncreaseRateMultiplier, -100);
        }
    }
}