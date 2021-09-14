namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateManufacturingSpeedMultiplier
        : BaseRateDouble<RateManufacturingSpeedMultiplier>
    {
        [NotLocalizable]
        public override string Description =>
            "Manufacturing speed for all manufacturers (such as furnaces, oil refineries, wells, mulchboxes, etc.)";

        public override string Id => "ManufacturingSpeedMultiplier";

        public override string Name => "Manufacturing speed";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0.1;

        public override double ValueMinReasonable => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}