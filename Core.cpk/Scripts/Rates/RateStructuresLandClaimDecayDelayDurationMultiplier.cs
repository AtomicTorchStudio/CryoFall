namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateStructuresLandClaimDecayDelayDurationMultiplier
        : BaseRateDouble<RateStructuresLandClaimDecayDelayDurationMultiplier>
    {
        [NotLocalizable]
        public override string Description =>
            @"Time multiplier before an abandoned land claim (or base) will start decaying.
              For example, the default decay delay for the land claims (T1) is 32 hours,
              but with 2.0 multiplier it will be increased to 64 hours (for T1).
              To ensure that your server will not have unexpected decay the default multiplier here is high.
              If you wish to use the decay duration exactly as it's defined in the land claims, set 1.0.";

        public override string Id => "Structures.LandClaimDecay.DelayDurationMultiplier";

        public override string Name => "[Decay] Abandoned land claim decay delay duration";

        public override IRate OrderAfterRate
            => this.GetRate<RateStructuresDecayEnabled>();

        public override double ValueDefault => 5.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 10.0;

        public override double ValueMin => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}