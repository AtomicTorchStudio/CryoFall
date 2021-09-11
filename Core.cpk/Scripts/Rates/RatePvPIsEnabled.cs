namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RatePvPIsEnabled
        : BaseRateBoolean<RatePvPIsEnabled>
    {
        [NotLocalizable]
        public override string Description =>
            @"Set it to 1 to make this server PvP. Otherwise it will be PvE.";

        public override string Id => "PvP";

        public override string Name => "PvP";

        public override bool ValueDefault => Api.IsEditor; // PvP by default is enabled only in Editor

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}