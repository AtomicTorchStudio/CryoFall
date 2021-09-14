namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi;

    public class RatePvPShieldProtectionEnabled
        : BaseRateBoolean<RatePvPShieldProtectionEnabled>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines whether S.H.I.E.L.D. base protection is available.";

        public override string Id => "PvP.ShieldProtection.Enabled";

        public override string Name => "[PvP] Base S.H.I.E.L.D. protection available";

        public override bool ValueDefault => true;

        public override RateVisibility Visibility => RateVisibility.Advanced;

        protected override bool ServerReadValue()
        {
            var result = base.ServerReadValue();
            if (PveSystem.ServerIsPvE)
            {
                // shield protection is not required in PvE as the base cannot be attacked
                result = false;
            }

            return result;
        }
    }
}