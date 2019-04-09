namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;

    public class SpawnConfig : BaseZoneScriptConfig<SpawnConfig>
    {
        public SpawnConfig(
            ProtoZoneSpawnScript zoneScript,
            double densityMultiplier) : base(zoneScript)
        {
            if (double.IsInfinity(densityMultiplier)
                || double.IsNaN(densityMultiplier))
            {
                throw new ArgumentException("Invalid density multiplier", nameof(densityMultiplier));
            }

            this.DensityMultiplier = densityMultiplier;
        }

        public double DensityMultiplier { get; }
    }
}