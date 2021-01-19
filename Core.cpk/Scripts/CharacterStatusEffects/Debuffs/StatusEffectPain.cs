namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class StatusEffectPain : ProtoStatusEffect
    {
        public const double MinimumIntensityWhenLowHealth = 0.1;

        /// <summary>
        /// Pain intensity drops quickly when it's high.
        /// </summary>
        public const double PainIntensityAutoDecreasePerSecondFraction
            = 1.0 / 100.0; // drops on 1% (from current pain intensity) per second

        /// <summary>
        /// Additionally pain intensity drops every second on a fixed amount.
        /// </summary>
        private const double PainIntensityAutoDecreasePerSecondValue
            = 2.0 / 100.0; // drops on 2% (fixed amount) per second 

        public override string Description =>
            "You are in severe pain, which reduces your stamina regeneration and prevents health regeneration. Find some painkillers or tough it out.";

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Severe pain";

        public override double ServerUpdateIntervalSeconds => 1.0;

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectPainManager.TargetIntensity = 0;
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectPainManager.TargetIntensity = data.Intensity;
        }

        protected override void PrepareEffects(Effects effects)
        {
            // energy regeneration -75%
            effects.AddPercent(this, StatName.StaminaRegenerationPerSecond, -75);

            // health regeneration -25%
            effects.AddPercent(this, StatName.HealthRegenerationPerSecond, -25);

            // more psi damage while under pain
            effects.AddPercent(this, StatName.PsiEffectMultiplier, 25);
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            intensityToAdd *= data.Character.SharedGetFinalStatMultiplier(StatName.PainIncreaseRateMultiplier);

            if (intensityToAdd <= 0)
            {
                return;
            }

            // otherwise add as normal
            base.ServerAddIntensity(data, intensityToAdd);
        }

        protected override IEnumerable<ICharacter> ServerAutoAddGetCharacterCandidates()
        {
            return Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
        }

        protected override void ServerOnAutoAdd(ICharacter character)
        {
            // we're auto-adding pain effects only to the characters with low health and who don't have the pain status effect
            if (!character.SharedHasStatusEffect<StatusEffectPain>()
                && IsLowHealth(character))
            {
                character.ServerAddStatusEffect(this, MinimumIntensityWhenLowHealth);
            }
        }

        /// <summary>
        /// Please note that this method is called once a second (see ServerUpdateIntervalSeconds)
        /// so multiplication on deltaTime is not necessary.
        /// </summary>
        protected override void ServerUpdate(StatusEffectData data)
        {
            // calculate new intensity
            var newIntensity = data.Intensity * (1 - PainIntensityAutoDecreasePerSecondFraction)
                               - PainIntensityAutoDecreasePerSecondValue;
            var minIntensity = IsLowHealth(data.Character) ? MinimumIntensityWhenLowHealth : 0;
            data.Intensity = Math.Max(minIntensity, newIntensity);
        }

        private static bool IsLowHealth(ICharacter character)
        {
            var stats = character.GetPublicState<ICharacterPublicState>().CurrentStats;
            var healthFraction = stats.HealthCurrent / stats.HealthMax;
            return healthFraction <= 0.1;
        }
    }
}