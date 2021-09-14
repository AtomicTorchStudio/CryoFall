namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateFarmPlantsGrowthSpeedMultiplier
        : BaseRateDouble<RateFarmPlantsGrowthSpeedMultiplier>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines how fast player planted farm plants grow.
              Note: changing this rate mid-game won't apply to the existing
              plants until harvested, watered, or fertilizer applied.";

        public override string Id => "FarmPlantsGrowthSpeedMultiplier";

        public override string Name => "Farm plants growth speed";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}