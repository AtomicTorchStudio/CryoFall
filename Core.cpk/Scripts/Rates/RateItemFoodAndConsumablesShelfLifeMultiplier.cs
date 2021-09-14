namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateItemFoodAndConsumablesShelfLifeMultiplier
        : BaseRateDouble<RateItemFoodAndConsumablesShelfLifeMultiplier>
    {
        [NotLocalizable]
        public override string Description =>
            "Adjusts the shelf life of the food, drinks, and other consumable items.";

        public override string Id => "ItemFoodAndConsumablesShelfLifeMultiplier";

        public override string Name => "Food and consumables shelf life";

        public override IRate OrderAfterRate
            => this.GetRate<RateThirst>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0.1;

        public override double ValueMinReasonable => 0.5;

        public override double ValueStepChange => 0.25;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}