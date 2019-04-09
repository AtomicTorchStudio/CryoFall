namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class PlantPrivateState : VegetationPrivateState
    {
        public IProtoItemFertilizer AppliedFertilizerProto { get; set; }

        public double LastWateringDuration { get; set; }

        public byte ProducedHarvestsCount { get; set; }

        public float ServerTimeLastDurationSeconds { get; set; }

        public double ServerTimeWateringEnds { get; set; }

        public double SkillGrowthSpeedMultiplier { get; set; } = 1d;
    }
}