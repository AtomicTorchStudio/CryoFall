namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateDepositsExtractionSpeedMultiplier
        : BaseRateDouble<RateDepositsExtractionSpeedMultiplier>
    {
        [NotLocalizable]
        public override string Description => "Deposits extraction rate for oil/Li extractors.";

        public override string Id => "Resources.DepositsExtractionSpeedMultiplier";

        public override string Name => "[Resources] Deposits extraction speed";

        public override IRate OrderAfterRate
            => this.GetRate<RateManufacturingSpeedMultiplier>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0.1;

        public override double ValueMinReasonable => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}