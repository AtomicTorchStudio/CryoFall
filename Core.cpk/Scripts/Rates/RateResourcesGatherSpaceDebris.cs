namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateResourcesGatherSpaceDebris
        : BaseRateDouble<RateResourcesGatherSpaceDebris>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines the resources gathering rate from space debris during the event.
              Increasing this number will provide more resources.";

        public override string Id => "Resources.Gather.SpaceDebris";

        public override string Name => "[Resources] Gather—Space debris";

        public override IRate OrderAfterRate
            => this.GetRate<RateResourcesGatherCreaturesLoot>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}