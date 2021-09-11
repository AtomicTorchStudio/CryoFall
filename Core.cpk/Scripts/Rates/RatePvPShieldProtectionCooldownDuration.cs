namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RatePvPShieldProtectionCooldownDuration
        : BaseRateInt<RatePvPShieldProtectionCooldownDuration>
    {
        [NotLocalizable]
        public override string Description =>
            @"Cannot reactivate a deactivated base S.H.I.E.L.D. for this duration (in seconds).";

        public override string Id => "PvP.ShieldProtection.CooldownDuration";

        public override string Name => "[PvP] Base S.H.I.E.L.D. protection cooldown duration";

        public override IRate OrderAfterRate
            => this.GetRate<RatePvPShieldProtectionActivationDuration>();

        public override int ValueDefault => 30 * 60; // 30 minutes

        public override int ValueMax => 60 * 60; // 1 hour

        public override int ValueMin => 0; // no limit

        public override RateValueType ValueType => RateValueType.DurationSeconds;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}