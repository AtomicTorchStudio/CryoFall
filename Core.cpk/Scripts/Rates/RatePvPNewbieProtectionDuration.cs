namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RatePvPNewbieProtectionDuration
        : BaseRateInt<RatePvPNewbieProtectionDuration>
    {
        [NotLocalizable]
        public override string Description =>
            @"Newbie protection duration (in seconds).
              Default value: 7200 seconds (2 hours). Don't set it higher than 2 billions.
              If you set it to 0 the Newbie Protection will be not applied to the new players.";

        public override string Id => "PvP.NewbieProtectionDuration";

        public override string Name => "[PvP] Newbie protection duration";

        public override int ValueDefault => 2 * 60 * 60; // 2 hours

        public override int ValueMax => 2_000_000_000; // unlimited

        public override int ValueMin => 0; // no protection

        public override RateValueType ValueType => RateValueType.DurationSeconds;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}