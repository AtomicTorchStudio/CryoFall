namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RateResourcesPvPDepositClaimDelay
        : BaseRateUint<RateResourcesPvPDepositClaimDelay>
    {
        [NotLocalizable]
        public override string Description =>
            @"Delay (in seconds) before the spawned resource deposit could be claimed on a PvP server.
              The notification about the spawned resource is displayed with this timer.
              If you change this to 0 there will be no resource spawn notification (only a map mark will be added).";

        public override string Id => "Resources.PvP.DepositClaimingDelay";

        public override string Name => "[Resources] [PvP] Deposit claiming delay";

        public override uint ValueDefault => Api.IsEditor
                                                 ? 0u
                                                 : 20 * 60; // 20 minutes

        public override uint ValueMax => 2 * 60 * 60; // 2 hours

        public override uint ValueMin => 0;

        public override RateValueType ValueType => RateValueType.DurationSeconds;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}