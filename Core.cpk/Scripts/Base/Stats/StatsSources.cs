namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data;

    public struct StatsSources
    {
        private List<StatEntry> list;

        public IReadOnlyList<StatEntry> List
            => this.list
               ?? (IReadOnlyList<StatEntry>)Array.Empty<StatEntry>();

        public static void Merge(ref StatsSources current, StatsSources other)
        {
            if (other.list is null)
            {
                return;
            }

            if (current.list is null)
            {
                current.list = other.List.ToList();
            }
            else
            {
                current.list.AddRange(other.list);
            }
        }

        public static void RegisterPercent(
            ref StatsSources stats,
            IProtoEntity source,
            StatName statName,
            double percent)
        {
            stats = stats.AddOrUpdate(source, statName, value: 0, percent: percent);
        }

        public static void RegisterValue(
            ref StatsSources stats,
            IProtoEntity source,
            StatName statName,
            double value)
        {
            stats = stats.AddOrUpdate(source, statName, value, percent: 0);
        }

        public void Clear()
        {
            if (this.list?.Count > 0)
            {
                this.list.Clear();
            }
        }

        public StatsSources Clone()
        {
            if (this.list is null)
            {
                return new StatsSources();
            }

            return new StatsSources()
            {
                list = this.list.ToList()
            };
        }

        private StatsSources AddOrUpdate(
            IProtoEntity source,
            StatName statName,
            double value,
            double percent)
        {
            this.list ??= new List<StatEntry>();

            for (var index = 0; index < this.list.Count; index++)
            {
                var entry = this.list[index];
                if (entry.StatName == statName
                    && entry.Source == source)
                {
                    // found existing entry - update it
                    entry.Value += value;
                    entry.Percent += percent;
                    this.list[index] = entry;
                    return this;
                }
            }

            // create new entry
            this.list.Add(new StatEntry(source, statName, value, percent));
            return this;
        }

        public struct StatEntry
        {
            public readonly IProtoEntity Source;

            public readonly StatName StatName;

            public double Percent;

            public double Value;

            public StatEntry(IProtoEntity source, StatName statName, double value, double percent)
            {
                this.Source = source;
                this.StatName = statName;
                this.Value = value;
                this.Percent = percent;
            }
        }
    }
}