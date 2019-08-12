using AtomicTorch.CBND.CoreMod.Stats;

namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    public class StatusEffectProtectionRadiation : ProtoStatusEffect
    {
        public override string Description =>
            "You are currently under the effect of radiation protection and any new radiation exposure has less of an effect on you.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Radiation protection";

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.RadiationPoisoningIncreaseRateMultiplier, -50);
        }
    }
}