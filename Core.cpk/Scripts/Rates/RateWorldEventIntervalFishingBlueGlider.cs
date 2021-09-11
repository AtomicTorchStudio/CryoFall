namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class RateWorldEventIntervalFishingBlueGlider
        : BaseRateWorldEventInterval<EventFishingBlueGlider, RateWorldEventIntervalFishingBlueGlider>
    {
        public override Interval<double> DefaultTimeIntervalHours => (min: 5, max: 7);
    }
}