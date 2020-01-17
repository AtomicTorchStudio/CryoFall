namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.Noise;

    public class SpawnRequest
    {
        // please note that this density value is already including the density multiplier
        public readonly double Density;

        public readonly int DesiredCount;

        public readonly ObjectSpawnPreset Preset;

        public SpawnRequest(
            ObjectSpawnPreset preset,
            int desiredCount,
            int currentCount,
            int spawnCount,
            double density,
            bool useSectorDensity)
        {
            this.Preset = preset;
            this.DesiredCount = desiredCount;
            this.CurrentCount = currentCount;
            this.CountToSpawn = spawnCount;
            this.Density = density;
            this.UseSectorDensity = useSectorDensity;
        }

        public int CountToSpawn { get; private set; }

        public int CurrentCount { get; private set; }

        public int FailedAttempts { get; set; }

        public int SpawnedCount { get; private set; }

        public bool UseSectorDensity { get; }

        public void OnSpawn()
        {
            this.CountToSpawn--;
            this.CurrentCount++;
            this.SpawnedCount++;
        }
    }
}