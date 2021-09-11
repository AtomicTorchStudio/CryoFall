namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class RateWorldEventIntervalPsiGroveInfestation
        : BaseRateWorldEventInterval<EventPsiGroveInfestation, RateWorldEventIntervalPsiGroveInfestation>
    {
        public override Interval<double> DefaultTimeIntervalHours => (min: 4, max: 6);
    }
}