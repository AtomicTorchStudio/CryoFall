namespace AtomicTorch.CBND.CoreMod.Stats
{
    using AtomicTorch.CBND.GameApi.Data;

    /// <summary>
    /// Provides methods to only setting values.
    /// Then it must be merged into a <see cref="TempStatsCache" /> to get <see cref="FinalStatsCache" /> via
    /// <see cref="BaseStatsDictionary.CalculateFinalStatsCache" />.
    /// </summary>
    public sealed class Effects : BaseStatsDictionary
    {
        public Effects()
        {
            this.IsMultipliersSummed = true;
        }

        public void AddPerk(IProtoEntity source, StatName statName)
        {
            this.AddValue(source, statName, value: 1);
        }

        public IReadOnlyStatsDictionary ToReadOnly()
        {
            this.MakeReadOnly();
            return this;
        }
    }
}