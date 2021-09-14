namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateResourcesGatherBasic
        : BaseRateDouble<RateResourcesGatherBasic>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines the resources and loot gathering rate.
              Increasing this number will provide more items when foraging, cutting trees,
              mining minerals, searching garbage piles, opening crates in ruins,
              gathering harvest, etc.";

        public override string Id => "Resources.Gather.Basic";

        public override string Name => "[Resources] Gather—Basic gathering";

        public override IRate OrderAfterRate
            => this.GetRate<RateSkillExperienceGainMultiplier>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}