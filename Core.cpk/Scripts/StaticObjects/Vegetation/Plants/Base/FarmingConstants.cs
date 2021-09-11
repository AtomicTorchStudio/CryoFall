namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public static class FarmingConstants
    {
        public const double WateringGrowthSpeedMultiplier = 2.0;

        public static double ServerFarmPlantsGrowthSpeedMultiplier
            => RateFarmPlantsGrowthSpeedMultiplier.SharedValue;

        public static double SharedFarmPlantsLifetimeMultiplier
            => RateFarmPlantsLifetimeMultiplier.SharedValue;
    }
}