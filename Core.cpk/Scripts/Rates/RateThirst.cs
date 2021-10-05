namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateThirst
        : BaseRateDouble<RateThirst>
    {
        [NotLocalizable]
        public override string Description =>
            @"Water consumption speed multiplier for thirst mechanic.
              By default the game will consume 100 water points in 1 hour,
              you can make it faster or slower, or disable altogether.
              Please note the game will consume water twice as fast if player's stamina is regenerating.
              Example: setting it to 2.0 will make your thirst twice as fast.";

        public override string Id => "ThirstRate";

        public override string Name => "Thirst rate";

        public override IRate OrderAfterRate
            => this.GetRate<RateHunger>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0;

        public override double ValueStepChange => 0.1;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}