namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectBleeding : ProtoStatusEffect
    {
        public const double DamagePerSecondBase = 0.1;

        public const double DamagePerSecondByIntensity = 1;

        public override string Description =>
            "You are bleeding, which will cause you to continuously lose health if not treated. Minor bleeding will go away on its own, but more serious injuries might require use of bandages or hemostatic medicine.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max bleeding

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Bleeding";

        protected override void PrepareEffects(Effects effects)
        {
            // no health regeneration while bleeding
            effects.AddPercent(this, StatName.HealthRegenerationPerSecond, -100);

            // add info to tooltip that this effect deals damage
            effects.AddValue(this, StatName.VanityContinuousDamage, 1);
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            intensityToAdd *= data.Character.SharedGetFinalStatMultiplier(StatName.BleedingIncreaseRateMultiplier);
            // new intensity is calculated with hypotenuse formula
            data.Intensity = Math.Sqrt(data.Intensity * data.Intensity
                                       + intensityToAdd * intensityToAdd);
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            // basic damage has a fixed component plus is affected by intensity
            var damage = (DamagePerSecondBase + DamagePerSecondByIntensity * data.Intensity)
                         * data.DeltaTime;

            data.CharacterCurrentStats.ServerReduceHealth(damage, this);
        }
    }
}