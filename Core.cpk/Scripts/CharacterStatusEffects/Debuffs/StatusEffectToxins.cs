namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
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

            // add info to tooltip that this effect deals damage
            effects.AddValue(this, StatName.VanityContinuousDamage, 1);
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            intensityToAdd *= data.Character.SharedGetFinalStatMultiplier(StatName.ToxinsIncreaseRateMultiplier);
            // new intensity is calculated with hypotenuse formula
            data.Intensity = Math.Sqrt(data.Intensity * data.Intensity
                                       + intensityToAdd * intensityToAdd);
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            var damage = (DamagePerSecondBase + DamagePerSecondByIntensity * data.Intensity)
                         * data.DeltaTime;

            data.CharacterCurrentStats.ServerReduceHealth(damage, this);
        }
    }
}