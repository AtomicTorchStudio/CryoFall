namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public struct ProtoPlantTooltipPrivateData : IRemoteCallParameter
    {
        public readonly IProtoItemFertilizer AppliedFertilzerProto;

        public readonly double LastWateringDuration;

        public readonly byte ProducedHarvestsCount;

        public readonly double ServerTimeNextHarvest;

        public readonly double ServerTimeWateringEnds;

        public readonly float SkillGrowthSpeedMultiplier;

        public readonly float SpeedMultiplier;

        public ProtoPlantTooltipPrivateData(
            PlantPrivateState privateState,
            double serverTimeNextHarvest,
            float speedMultiplier)
        {
            this.ProducedHarvestsCount = privateState.ProducedHarvestsCount;
            this.AppliedFertilzerProto = privateState.AppliedFertilizerProto;
            this.ServerTimeWateringEnds = privateState.ServerTimeWateringEnds;
            this.LastWateringDuration = privateState.LastWateringDuration;
            this.SkillGrowthSpeedMultiplier = (float)privateState.SkillGrowthSpeedMultiplier;
            this.ServerTimeNextHarvest = serverTimeNextHarvest;
            this.SpeedMultiplier = speedMultiplier;
        }
    }
}