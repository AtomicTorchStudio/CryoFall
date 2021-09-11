namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class RateWorldEventIntervalSpaceDrop
        : BaseRateWorldEventInterval<EventSpaceDrop, RateWorldEventIntervalSpaceDrop>
    {
        public override Interval<double> DefaultTimeIntervalHours => (min: 2.5, max: 4.0);
    }
}