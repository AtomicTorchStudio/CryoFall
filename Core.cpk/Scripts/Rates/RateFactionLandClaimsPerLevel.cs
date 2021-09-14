namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateFactionLandClaimsPerLevel
        : BaseRateDouble<RateFactionLandClaimsPerLevel>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines how many land claims each faction level provides.
              Total number is calculated as a faction level multiplied by this rate,
              then rounded to the nearest integer number.";

        public override string Id => "Faction.LandClaimsPerLevel";

        public override string Name => "[Faction] Faction-owned land claims per level";

        public override double ValueDefault => 1.2;

        public override double ValueMax => 20;

        public override double ValueMin => 0;

        public override double ValueStepChange => 0.1;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}