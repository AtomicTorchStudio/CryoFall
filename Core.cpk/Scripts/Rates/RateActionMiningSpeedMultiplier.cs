namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RateActionMiningSpeedMultiplier
        : BaseRateDouble<RateActionMiningSpeedMultiplier>
    {
        public override string Description =>
            "Adjusts the damage to minerals by tools and drones.";

        public override string Id => "Action.MiningSpeedMultiplier";

        public override string Name => "Mining speed";

        public override IRate OrderAfterRate
            => this.GetRate<RateCraftingSpeedMultiplier>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}