namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateFarmPlantsLifetimeMultiplier
        : BaseRateDouble<RateFarmPlantsLifetimeMultiplier>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines the farm plants lifetime.
              To make plants spoil twice slower - set it 2.0, alternatively to make them spoil twice as fast set it to 0.5.
              (it doesn't apply to the already planted plants until harvested, watered, or fertilizer applied)";

        public override string Id => "FarmPlantsLifetimeMultiplier";

        public override string Name => "Farm plants lifetime";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0.1;

        public override double ValueMinReasonable => 0.5;

        public override double ValueStepChange => 0.25;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}