namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateResourcesGatherCratesLoot
        : BaseRateDouble<RateResourcesGatherCratesLoot>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines the loot gathering rate from crates (such as loot crates in ruins/radtowns).
              Increasing this number will provide more items when searching crates and piles.";

        public override string Id => "Resources.Gather.CratesLoot";

        public override string Name => "[Resources] Gather—Crates and piles";

        public override IRate OrderAfterRate
            => this.GetRate<RateResourcesGatherCreaturesLoot>();

        public override double ValueDefault => 1.0;

        public override double ValueMax => 100.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 1.0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}