namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectBleeding : ProtoStatusEffect
    {
        // double damage to creatures as they have way more HP in comparison to player characters
        public const double DamageMultiplierPvE = 5.0;

        public const double DamagePerSecondBase = 0.1;

        public const double DamagePerSecondByIntensity = 1;

        // 5x faster effect to creatures as nobody in their sane mind will wait 5 minutes to kill a wolf
        public const double EffectDecaySpeedMultiplierPvE = 5.0;

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
            if (data.Character.IsNpc)
            {
                // faster effect's decay for creatures
                delta *= EffectDecaySpeedMultiplierPvE;
            }

            intensity -= delta;

            data.Intensity = intensity;
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            // basic damage has a fixed component plus is affected by intensity
            var damage = (DamagePerSecondBase + DamagePerSecondByIntensity * data.Intensity)
                         * data.DeltaTime;

            if (data.Character.IsNpc)
            {
                // more damage to creatures
                damage *= DamageMultiplierPvE;
            }

            data.CharacterCurrentStats.ServerReduceHealth(damage, data.StatusEffect);
        }
    }
}