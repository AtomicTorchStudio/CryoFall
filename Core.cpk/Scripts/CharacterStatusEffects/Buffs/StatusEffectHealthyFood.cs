namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectHealthyFood : ProtoStatusEffect
    {
        public override string Description =>
            "You've recently eaten a very healthy meal full of vitamins, microelements and other important nutrients. You feel great.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Buff;

        public override string Name => "Healthy food";

        protected override void PrepareEffects(Effects effects)
        {
            // small bonuses in many areas
            effects.AddValue(this, StatName.HealthMax, 10);
            effects.AddPercent(this, StatName.HealthRegenerationPerSecond,              25);
            effects.AddPercent(this, StatName.StaminaRegenerationPerSecond,             25);
            effects.AddPercent(this, StatName.RadiationPoisoningIncreaseRateMultiplier, -10);
            effects.AddPercent(this, StatName.ToxinsIncreaseRateMultiplier,             -10);
            effects.AddPercent(this, StatName.BleedingIncreaseRateMultiplier,           -10);
            effects.AddPercent(this, StatName.PainIncreaseRateMultiplier,               -10);
        }
    }
}