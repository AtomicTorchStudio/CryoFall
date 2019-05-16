namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Invisible;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectRadiationPoisoning : ProtoStatusEffect
    {
        public override string Description =>
            "You are suffering from radiation poisoning, which is slowly killing you. Find radiation medication to reduce its effects.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 300.0; // 5 minutes

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Radiation poisoning";

        public override double VisibilityIntensityThreshold => 0.05; // shows up only when it's above 5%

        protected override void PrepareEffects(Effects effects)
        {
            // no health regeneration while under effect of radiation
            effects.AddPercent(this, StatName.HealthRegenerationPerSecond, -100);
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            // check for radiation protection effect
            if (data.Character.SharedHasStatusEffect<StatusEffectProtectionRadiation>())
            {
                // the character has a radiation protection status effect
                intensityToAdd *= 0.5;
            }

            // apply the armor defense from radiation
            var defenseRadiation = data.Character.SharedGetFinalStatValue(StatName.DefenseRadiation);
            intensityToAdd *= Math.Max(0, 1 - defenseRadiation);

            // increase radiation effect if the player has radiation effect multiplier
            intensityToAdd *= data.Character.SharedGetFinalStatMultiplier(StatName.RadiationPoisoningAccumulationMultiplier);
            

            base.ServerAddIntensity(data, intensityToAdd);
        }

        protected override void ServerOnAutoDecreaseIntensity(StatusEffectData data)
        {
            var character = data.Character;
            if (!character.SharedHasStatusEffect<BaseStatusEffectEnvironmentalRadiation>())
            {
                // decrease intensity only when the player is not affected by radiation environmental radiation
                base.ServerOnAutoDecreaseIntensity(data);
            }
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            // damage is calculated progressively depending on intensity dmg = (A*100)^2.5 / 10000;
            var DamagePerSecond = Math.Pow(data.Intensity * 100, 2.5) / 10000;

            // increase the damage based on radiation poisoning damage multiplier
            DamagePerSecond *= data.Character.SharedGetFinalStatMultiplier(StatName.RadiationPoisoningDamageMultiplier);

            // reducing health
            var stats = data.CharacterCurrentStats;
            stats.ServerReduceHealth(DamagePerSecond * data.DeltaTime, this);
        }
    }
}