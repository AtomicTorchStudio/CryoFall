namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RateCraftingSpeedMultiplier
        : BaseRateDouble<RateCraftingSpeedMultiplier>
    {
        public override string Description =>
            @"Determines the crafting speed of recipes
              started from crafting menu or from any workbench.
              Does NOT apply to manufacturing structures (such as furnace) - see ManufacturingSpeedMultiplier.";

        public override string Id => "CraftingSpeedMultiplier";

        public override string Name => "Crafting speed";

        public override IRate OrderAfterRate
            => this.GetRate<RateResourcesGatherBasic>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0.1;

        public override double ValueMinReasonable => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}