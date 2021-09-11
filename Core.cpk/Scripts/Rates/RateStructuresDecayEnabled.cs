namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RateStructuresDecayEnabled
        : BaseRateBoolean<RateStructuresDecayEnabled>
    {
        [NotLocalizable]
        public override string Description =>
            @"Set it to 0 to disable structure decay.
              Set it to 1 to enable structure decay.";

        public override string Id => "Structures.DecayEnabled";

        public override string Name => "[Decay] Abandoned structures decay";

        public override bool ValueDefault => !Api.IsEditor; // allow decay by default unless running in Editor

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}