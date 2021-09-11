namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateResourcesGatherMeteorites
        : BaseRateDouble<RateResourcesGatherMeteorites>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines the resources gathering rate from meteorites during the event.
              Increasing this number will provide more resources.";

        public override string Id => "Resources.Gather.Meteorites";

        public override string Name => "[Resources] Gather — Meteorites";

        public override IRate OrderAfterRate
            => this.GetRate<RateResourcesGatherSpaceDebris>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}