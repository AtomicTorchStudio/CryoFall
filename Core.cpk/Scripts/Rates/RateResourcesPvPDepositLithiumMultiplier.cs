namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RateResourcesPvPDepositLithiumMultiplier
        : BaseRateDouble<RateResourcesPvPDepositLithiumMultiplier>
    {
        public override string Description =>
            @"This multiplier determines how many lithium deposits should be present in each suitable biome.
              E.g. to reduce the max number of geothermal springs to half, change this to 0.5.
              You can also completely disable the spawn of Li deposits by changing this to 0.
              Please note: it's available only in PvP. (in PvE there is no need for deposits to capture). 
              It's not possible to define a specific number as it depends on the biome and map size.";

        public override string Id => "Resources.PvP.DepositLithiumMultiplier";

        public override string Name => "[Resources] [PvP] Lithium deposits number";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 5.0;

        public override double ValueMin => 0;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}