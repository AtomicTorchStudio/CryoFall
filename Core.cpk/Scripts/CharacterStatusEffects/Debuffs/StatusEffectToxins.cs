namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectToxins : ProtoStatusEffect
    {
        public const double DamagePerSecondBase = 0.25;

        public const double DamagePerSecondByIntensity = 0.75;

        public override string Description =>
            "You have been exposed to dangerous toxins. Use anti-toxin medicine to prevent further health loss.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 300.0; // total of 5 minutes for max toxins

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Toxins";

        protected override void PrepareEffects(Effects effects)
        {
            // no health regeneration while affected by toxins
            effects.AddPercent(this, StatName.HealthRegenerationPerSecond, -100);
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            // does the character has toxin protection?
            if (data.Character.SharedHasStatusEffect<StatusEffectProtectionToxins>())
            {
                intensityToAdd *= 0.5;
            }

            // exponent to use when adding the two values together using "hypotenuse" algorithm
            var exponent = 2;

            // calculating the new intensity
            var newIntensity = Math.Sqrt(Math.Pow(data.Intensity, exponent) + Math.Pow(intensityToAdd, exponent));

            // saving new intensity
            data.Intensity = newIntensity;
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            // damage
            var damage = (DamagePerSecondBase + DamagePerSecondByIntensity * data.Intensity) * data.DeltaTime;

            // reduce character health
            var stats = data.CharacterCurrentStats;
            var newHealth = stats.HealthCurrent - damage;

            stats.ServerSetHealthCurrent((float)newHealth);
        }
    }
}