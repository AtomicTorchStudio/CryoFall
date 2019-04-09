namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Invisible
{
    using System;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class StatusEffectEnvironmentalRadiationTier3 : BaseStatusEffectEnvironmentalRadiation
    {
        protected override IProtoZone ServerZoneProto => Api.GetProtoEntity<ZoneSpecialRadiationTier3>();

        /// <summary>
        /// This method is called only once per second per each character in the radiation zone.
        /// RadiationIntensity is current environmental radiation (0-1).
        /// Please return value you would like to add to the radiation poisoning status effect.
        /// Please note this value will be decreased later at
        /// <see cref="ServerAddIntensity" />.
        /// </summary>
        protected override double CalculatePoisoningIntensityToAdd(
            double environmentalRadiationIntensity,
            double currentEffectIntensity)
        {
            // intensity threshold for this radiation zone (effectively target radiation level)
            double threshold = 0.55;

            // what fraction of remaining distance to threshold value is added every second?
            double thresholdFractionIncrease = 0.05; // 5%

            // and now calculate how much is added based on threshold and current player radiation level
            double IncreaseFromThreshold = Math.Max(0, threshold - currentEffectIntensity) * thresholdFractionIncrease;

            // normal flat increase that ignores threshold and current rad level
            double IncreaseNormal = environmentalRadiationIntensity / 60.0 / 3.0; // 3 minutes to reach 100% intensity

            // and finally combine the two and return the increase value
            return IncreaseFromThreshold + IncreaseNormal;
        }
    }
}