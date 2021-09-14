namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateLandClaimOwnersMax
        : BaseRateByte<RateLandClaimOwnersMax>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines the max number of land claim's owners (including the founder)
              for land claims that are not transferred to faction ownership.
              If you want to make a server where players must group in factions,
              set it to 1 and reduce the faction create cost.
              Pleases note: if you change this to 1 owner, the access list will be hidden altogether.";

        public override string Id => "LandClaimOwnersMax";

        public override string Name => "Max land claim owners (non-faction ownership)";

        public override byte ValueDefault => 5;

        public override byte ValueMax => 100;

        public override byte ValueMin => 1;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}