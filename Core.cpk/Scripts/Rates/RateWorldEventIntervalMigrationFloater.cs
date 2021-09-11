namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class RateWorldEventIntervalMigrationFloater
        : BaseRateWorldEventInterval<EventMigrationFloater, RateWorldEventIntervalMigrationFloater>
    {
        public override Interval<double> DefaultTimeIntervalHours => (min: 6, max: 9);
    }
}