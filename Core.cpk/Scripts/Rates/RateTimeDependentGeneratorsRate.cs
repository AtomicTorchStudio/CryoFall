namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateTimeDependentGeneratorsRate
        : BaseRateDouble<RateTimeDependentGeneratorsRate>
    {
        [NotLocalizable]
        public override string Description =>
            @"This rate determines how fast the slow generators (such as solar and pragmium power plants)
              produce power and consumes the fuel (such as pragmium rods or simple solar panels).
              The intention of this rate is to make the game playable in single player when players
              don't have much time to wait and they cannot keep the local server running 24/7 like
              a multiplayer server should operate. E.g. to make the game enjoyable on a local server
              configure it to a relatively high number such as 3.0 (so the power will be produced
              3 times as fast but consume the fuel/panels 3 times faster as well).";

        public override string Id => "TimeDependentGeneratorsRate";

        public override string Name => "Time dependent generators rate";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 1;

        public override double ValueStepChange => 0.25;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}