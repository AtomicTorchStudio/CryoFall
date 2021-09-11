namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class RateWorldEventIntervalMeteoriteDrop
        : BaseRateWorldEventInterval<EventMeteoriteDrop, RateWorldEventIntervalMeteoriteDrop>
    {
        public override Interval<double> DefaultTimeIntervalHours => (min: 2.5, max: 3.5);
    }
}