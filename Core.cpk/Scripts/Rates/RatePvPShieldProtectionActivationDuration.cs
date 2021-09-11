namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RatePvPShieldProtectionActivationDuration
        : BaseRateInt<RatePvPShieldProtectionActivationDuration>
    {
        [NotLocalizable]
        public override string Description =>
            @"Base S.H.I.E.L.D. activation duration (in seconds).";

        public override string Id => "PvP.ShieldProtection.ActivationDuration";

        public override string Name => "[PvP] Base S.H.I.E.L.D. protection activation duration";

        public override IRate OrderAfterRate
            => this.GetRate<RatePvPShieldProtectionEnabled>();

        public override int ValueDefault => 15 * 60; // 15 minutes

        public override int ValueMax => 60 * 60; // 1 hour

        public override int ValueMin => 0; // instant activation

        public override RateValueType ValueType => RateValueType.DurationSeconds;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}