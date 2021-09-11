namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RatePvPIsResourceDepositCoordinatesHidden
        : BaseRateBoolean<RatePvPIsResourceDepositCoordinatesHidden>
    {
        [NotLocalizable]
        public override string Description =>
            @"Set it to 1 to hide the world coordinates for new resource deposit (oil/Li)
              until it's possible to capture it.
              When coordinates are hidden, players will see only a circle area on the map
              instead of the actual coordinates for the resource deposit.";

        public override string Id => "PvP.IsResourceDepositCoordinatesHidden";

        public override string Name => "[PvP] Hide coordinates for new resource deposits";

        public override bool ValueDefault => !Api.IsEditor; // hide except in Editor

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}