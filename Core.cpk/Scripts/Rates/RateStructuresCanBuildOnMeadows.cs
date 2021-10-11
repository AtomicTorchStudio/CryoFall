namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateStructuresCanBuildOnMeadows
        : BaseRateBoolean<RateStructuresCanBuildOnMeadows>
    {
        [NotLocalizable]
        public override string Description =>
            @"Set it to 0 to disallow placing structures on meadows.
              Set it to 1 to allow it.";

        public override string Id => "Structures.CanBuildOnMeadows";

        public override string Name => "[Structures] Can build on meadows";

        public override bool ValueDefault => true;

        public override RateVisibility Visibility => RateVisibility.Hidden;
    }
}