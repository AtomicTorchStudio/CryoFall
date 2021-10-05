namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateStructuresTextSignsAvailable
        : BaseRateBoolean<RateStructuresTextSignsAvailable>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines whether the text signs are available for research and can be placed.
              As there is no way to block offensive messages written by players on these signs,
              you may not wish to allow building them on a multiplayer server.";

        public override string Id => "Structures.TextSignsAvailable";

        public override string Name => "Text signs available";

        public override bool ValueDefault => true;

        public override RateVisibility Visibility => RateVisibility.Hidden;
    }
}