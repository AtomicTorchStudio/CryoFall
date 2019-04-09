namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectBleeding : ProtoStatusEffect
    {
        public const double DamagePerSecond = 1;

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
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            //does the character has bleeding protection?
            if (data.Character.SharedHasStatusEffect<StatusEffectProtectionBleeding>())
            {
                intensityToAdd *= 0.5;
            }

            // exponent to use when adding the two values together using "hypotenuse" alorithm
            var exponent = 2;

            // calculating the new intensity
            var newIntensity = Math.Sqrt(Math.Pow(data.Intensity, exponent) + Math.Pow(intensityToAdd, exponent));

            // saving new intensity
            data.Intensity = newIntensity;
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            // basic damage has a fixed component plus is affected by intensity
            var damage = (DamagePerSecond * data.Intensity + 0.1) * data.DeltaTime;

            // reduce character health
            var stats = data.CharacterCurrentStats;
            var newHealth = stats.HealthCurrent - damage;

            stats.ServerSetHealthCurrent((float)newHealth);
        }
    }
}