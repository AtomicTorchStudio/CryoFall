namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System.Collections.Generic;

    public interface IReadOnlyStatsDictionary
    {
        bool IsEmpty { get; }

        IReadOnlyDictionary<StatName, double> Multipliers { get; }

        StatsSources Sources { get; }

        IReadOnlyDictionary<StatName, double> Values { get; }
    }
}