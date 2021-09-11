namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RatePvPRaidBlockDuration
        : BaseRateInt<RatePvPRaidBlockDuration>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines the duration (in seconds) of ""raid block"" feature.
              - it prevents players from repairing and building new structures
              after a bomb has exploded within their land claim area.
              Applies only to bombs (except mining charge).";

        public override string Id => "PvP.RaidBlockDuration";

        public override string Name => "[PvP] Raid block duration";

        public override int ValueDefault => 10 * 60; // 10 minutes

        public override int ValueMax => 60 * 60; // 1 hour

        public override int ValueMin => 0; // no protection

        public override RateValueType ValueType => RateValueType.DurationSeconds;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}