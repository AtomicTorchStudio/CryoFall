namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateHunger
        : BaseRateDouble<RateHunger>
    {
        [NotLocalizable]
        public override string Description =>
            @"Food consumption speed multiplier for hunger mechanic. 
              By default the game will consume 100 food points in 1.2 hours,
              you can make it faster or slower, or disable altogether.
              Example: setting it to 2.0 will make your hungry twice as fast.";

        public override string Id => "HungerRate";

        public override string Name => "Hunger rate";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0;

        public override double ValueStepChange => 0.1;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}