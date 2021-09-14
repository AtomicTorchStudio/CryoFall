namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateActionWoodcuttingSpeedMultiplier
        : BaseRateDouble<RateActionWoodcuttingSpeedMultiplier>
    {
        [NotLocalizable]
        public override string Description =>
            "Adjusts the damage to trees by tools and drones.";

        public override string Id => "Action.WoodcuttingSpeedMultiplier";

        public override string Name => "Woodcutting speed";

        public override IRate OrderAfterRate
            => this.GetRate<RateActionMiningSpeedMultiplier>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}