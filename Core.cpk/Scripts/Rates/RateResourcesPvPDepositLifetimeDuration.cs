namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateResourcesPvPDepositLifetimeDuration
        : BaseRateDouble<RateResourcesPvPDepositLifetimeDuration>
    {
        [NotLocalizable]
        public override string Description =>
            "Defines the lifetime of oil/Li deposits which are available only on PvP servers.";

        public override string Id => "Resources.PvP.DepositLifetimeDuration";

        public override string Name => "[Resources] [PvP] Deposit lifetime duration";

        public override IRate OrderAfterRate
            => this.GetRate<RateResourcesPvPDepositClaimDelay>();

        public override double ValueDefault => 4 * 24 * 60 * 60; // 4 days

        public override double ValueMax => 12 * 24 * 60 * 60; // 12 days

        public override double ValueMaxReasonable => 6 * 24 * 60 * 60; // 6 days

        public override double ValueMin => 8 * 60 * 60; // 8 hours

        public override double ValueMinReasonable => 24 * 60 * 60; // 24 hours

        public override RateValueType ValueType => RateValueType.DurationSeconds;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}