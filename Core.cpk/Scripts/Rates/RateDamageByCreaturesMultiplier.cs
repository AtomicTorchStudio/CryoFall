namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RateDamageByCreaturesMultiplier
        : BaseRateDouble<RateDamageByCreaturesMultiplier>
    {
        public override string Description =>
            "All damage dealt by creatures (to player and/or other creatures) is multiplied by this rate.";

        public override string Id => "Damage.CreaturesMultiplier";

        public override string Name => "Damage by creatures";

        public override double ValueDefault => 1;

        public override double ValueMax => 10;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0;

        public override double ValueMinReasonable => 0.5;

        public override double ValueStepChange => 0.25;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}