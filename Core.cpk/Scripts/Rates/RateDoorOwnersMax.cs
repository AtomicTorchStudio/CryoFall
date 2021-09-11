namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateDoorOwnersMax
        : BaseRateByte<RateDoorOwnersMax>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines the max number of door's owners (including the builder).
              Min value: 1 owner.
              Max value: 255 owners (no limit displayed).";

        public override string Id => "DoorOwnersMax";

        public override string Name => "Door owners max number";

        public override byte ValueDefault => 5;

        public override byte ValueMax => 255;

        public override byte ValueMaxReasonable => 20;

        public override byte ValueMin => 1;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}