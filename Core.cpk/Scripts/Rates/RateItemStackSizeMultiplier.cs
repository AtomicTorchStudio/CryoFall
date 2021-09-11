namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateItemStackSizeMultiplier
        : BaseRateByte<RateItemStackSizeMultiplier>
    {
        [NotLocalizable]
        public override string Description =>
            @"Item stack size (capacity) multiplier.
              For example, by default one slot can contain up to 250 stone.
              You can increase this number by raising this multiplier.";

        public override string Id => "ItemStackSizeMultiplier";

        public override string Name => "Item stack size";

        public override byte ValueDefault => 1;

        public override byte ValueMax => 50;

        public override byte ValueMaxReasonable => 5;

        public override byte ValueMin => 1;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}