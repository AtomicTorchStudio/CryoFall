namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectLaceration : ProtoStatusEffect
    {
        public const double DamagePerSecondBase = 0.1;

        public const double DamagePerSecondByIntensity = 0.4;

        public override string Description =>
            "You have a very large open wound on your body. Such serious wounds normally require serious medical treatment.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Heavy laceration";

        protected override void PrepareEffects(Effects effects)
        {
            // add info to tooltip that this effect deals damage
            effects.AddValue(this, StatName.VanityContinuousDamage, 1);

            effects.AddPercent(this, StatName.HealthRegenerationPerSecond, -100);

            effects.AddPercent(this, StatName.BleedingIncreaseRateMultiplier, 50);

            effects.AddValue(this, StatName.StaminaMax, -10);
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            intensityToAdd *= data.Character.SharedGetFinalStatMultiplier(StatName.BleedingIncreaseRateMultiplier);
            // new intensity is calculated with hypotenuse formula
            data.Intensity = Math.Sqrt(data.Intensity * data.Intensity
                                       + intensityToAdd * intensityToAdd);
        }

        protected override void ServerOnAutoDecreaseIntensity(StatusEffectData data)
        {
            var intensity = data.Intensity;
            if (intensity <= 0)
            {
                if (!data.StatusEffect.IsDestroyed)
                {
                    data.Intensity = 0;
                }

                return;
            }

            var delta = this.IntensityAutoDecreasePerSecondValue * data.DeltaTime;
            intensity -= delta;

            data.Intensity = intensity;
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            // basic damage has a fixed component plus is affected by intensity
            var damage = (DamagePerSecondBase + DamagePerSecondByIntensity * data.Intensity)
                         * data.DeltaTime;

            data.CharacterCurrentStats.ServerReduceHealth(damage, data.StatusEffect);
        }
    }
}