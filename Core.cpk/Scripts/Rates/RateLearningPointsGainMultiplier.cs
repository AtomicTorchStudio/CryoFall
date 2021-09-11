namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RateLearningPointsGainMultiplier
        : BaseRateDouble<RateLearningPointsGainMultiplier>
    {
        public override string Description => "Determines the learning points acquisition rate.";

        public override string Id => "LearningPointsGainMultiplier";

        public override string Name => "Learning points";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0.1;

        public override double ValueMinReasonable => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}