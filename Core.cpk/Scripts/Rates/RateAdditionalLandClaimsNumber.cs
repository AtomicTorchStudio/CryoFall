namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateAdditionalLandClaimsNumber
        : BaseRateByte<RateAdditionalLandClaimsNumber>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines the extra number of land claims each player can own simultaneously.                                
              Currently in the game every player can build max 3 land claims.
              This rate allows to increase the number.
              Please note: it doesn't apply to faction-controlled land claims (see Faction.LandClaimsPerLevel).
              Min value: 0 (extra 0 land claims, that's the default value).
              Max value: 50 (extra 50 land claims).";

        public override string Id => "LandClaimsNumberLimitIncrease";

        public override string Name => "Additional land claims number";

        public override byte ValueDefault => 0;

        public override byte ValueMax => 50;

        public override byte ValueMin => 0;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}