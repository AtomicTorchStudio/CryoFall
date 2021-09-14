namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateResourcesPragmiumSourceMultiplier
        : BaseRateDouble<RateResourcesPragmiumSourceMultiplier>
    {
        [NotLocalizable]
        public override string Description =>
            @"This multiplier determines the number pragmium sources (large pillars)
              present in the Barren biome (desert).
              E.g. to reduce the max number of pragmium sources in half, change this to 0.5.
              Changing it to 2 will double the number (not guaranteed as they cannot spawn too close).
              You can also completely disable the spawn of pragmium in desert by changing this to 0.
              It's not possible to define a specific number as it depends on the biome and map size.
              (allowed range: from 0.0 to 10.0)";

        public override string Id => "Resources.PragmiumSourceMultiplier";

        public override string Name => "[Resources] Pragmium sources number";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}