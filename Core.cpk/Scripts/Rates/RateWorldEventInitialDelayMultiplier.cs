namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.GameApi;

    public class RateWorldEventInitialDelayMultiplier
        : BaseRateDouble<RateWorldEventInitialDelayMultiplier>
    {
        [NotLocalizable]
        public override string Description =>
            @"Determines the world event delay multiplier.
              Most events have a configured delay to prevent them from starting until players could advance enough.
              E.g. bosses will not spawn for the first 48 hours after the server wipe. 
              With this multiplier it's possible to adjust the delay to make events starting earlier or later
              (for example, if you want to get a boss spawn after 12 instead of 48 hours, set the multiplier to 0.25).
              Please note: on the PvP servers certain events like bosses will never start until
              the T4 specialized tech time gate is unlocked (as there is no viable way
              for players to defeat a boss until they can craft at least T4 weapons).";

        public override string Id => "WorldEvent.DelayMultiplier";

        public override string Name => "World events initial delay";

        public override double ValueDefault => 1.0;

        public override double ValueMax => 10.0;

        public override double ValueMaxReasonable => 1.0;

        public override double ValueMin => 0;

        public override double ValueStepChange => 0.05;

        public override RateValueType ValueType => RateValueType.Multiplier;

        public override RateVisibility Visibility => RateVisibility.Advanced;

        protected override double ServerReadValueWithRange()
        {
            var value = base.ServerReadValueWithRange();
            if (SharedLocalServerHelper.IsLocalServer)
            {
                value = 0.05; // enforce value for local server as larger value will cause issues
            }

            return value;
        }
    }
}