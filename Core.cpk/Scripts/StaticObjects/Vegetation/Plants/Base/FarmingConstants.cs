namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class FarmingConstants
    {
        public const double WateringGrowthSpeedMultiplier = 2.0;

        public static readonly double FarmPlantsGrowthSpeedMultiplier
            = ServerRates.Get(
                "FarmPlantsGrowthSpeedMultiplier",
                defaultValue: 1.0,
                @"This rate determines how fast the farm plants grow.
                  (to apply to the already planted plants please water them or add a fertilizer
                   so the growth duration will be recalculated)");

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                Logger.Important("Farm plants growth speed multiplier: "
                                 + FarmPlantsGrowthSpeedMultiplier.ToString("0.###"));
            }
        }
    }
}