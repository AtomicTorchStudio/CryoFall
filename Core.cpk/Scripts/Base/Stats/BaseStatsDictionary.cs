namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Data;

    public abstract class BaseStatsDictionary : IReadOnlyStatsDictionary
    {
        protected readonly Dictionary<StatName, double> Multipliers = new();

        protected readonly Dictionary<StatName, double> Values = new();

        private StatsSources sources;

        public bool IsEmpty => this.Values.Count == 0
                               && this.Multipliers.Count == 0;

        public bool IsReadOnly { get; private set; }

        public StatsSources Sources => this.sources;

        IReadOnlyDictionary<StatName, double> IReadOnlyStatsDictionary.Multipliers => this.Multipliers;

        IReadOnlyDictionary<StatName, double> IReadOnlyStatsDictionary.Values => this.Values;

        public BaseStatsDictionary AddPercent(IProtoEntity source, StatName statName, double percent)
        {
            if (percent == 0)
            {
                return this;
            }

            var multiplier = percent / 100.0;

            // simply sum multipliers (like values)
            if (multiplier == 0)
            {
                return this;
            }

            StatsSources.RegisterPercent(ref this.sources, source, statName, percent);

            if (this.Multipliers.TryGetValue(statName, out var currentMultiplier))
            {
                // simply sum with existing value
                multiplier += currentMultiplier;
            }
            else
            {
                // non-existing multiplier - add base multiplier (one)
                multiplier += 1;
            }

            this.ValidateIsNotReadOnly();
            this.Multipliers[statName] = multiplier;
            return this;
        }

        public BaseStatsDictionary AddValue(IProtoEntity source, StatName statName, double value)
        {
            if (value == 0)
            {
                return this;
            }

            StatsSources.RegisterValue(ref this.sources, source, statName, value);

            if (this.Values.TryGetValue(statName, out var currentValue))
            {
                // simply sum with existing value
                value += currentValue;
            }

            this.ValidateIsNotReadOnly();
            this.Values[statName] = value;
            return this;
        }

        public FinalStatsCache CalculateFinalStatsCache()
        {
            var finalMultipliers = new Dictionary<StatName, double>(this.Multipliers.Count);
            foreach (var pair in this.Multipliers)
            {
                var multiplier = pair.Value;
                multiplier = Math.Max(0, multiplier); // clamp multiplier so it cannot be negative
                finalMultipliers.Add(pair.Key, multiplier);
            }

            var finalValues = new Dictionary<StatName, double>(
                capacity: Math.Max(this.Values.Count, this.Multipliers.Count));
            foreach (var pair in this.Values)
            {
                var value = pair.Value;
                if (finalMultipliers.TryGetValue(pair.Key, out var multiplier))
                {
                    value *= multiplier;
                }

                finalValues[pair.Key] = value;
            }

            foreach (var noValueKey in finalMultipliers.Keys.Except(this.Values.Keys))
            {
                // we have a value with percent effect but without the initial value
                // assume the initial value is 1, so we can simply use the multiplier
                var multiplier = finalMultipliers[noValueKey];
                finalValues[noValueKey] = multiplier;
            }

            return new FinalStatsCache(finalValues, finalMultipliers, this.sources.Clone());
        }

        /// <summary>
        /// Merge values from other stats cache to this stats cache.
        /// </summary>
        public void Merge(IReadOnlyStatsDictionary otherStatsCache)
        {
            this.ValidateIsNotReadOnly();

            this.Merge(
                this.Values,
                otherStatsCache.Values,
                isMultipliers: false);

            this.Merge(
                this.Multipliers,
                otherStatsCache.Multipliers,
                isMultipliers: true);

            StatsSources.Merge(ref this.sources, otherStatsCache.Sources);
        }

        protected virtual void MakeReadOnly()
        {
            this.IsReadOnly = true;
        }

        protected void Merge(
            IDictionary<StatName, double> toDictionary,
            IReadOnlyDictionary<StatName, double> fromDictionary,
            bool isMultipliers)
        {
            foreach (var pair in fromDictionary)
            {
                if (toDictionary.TryGetValue(pair.Key, out var value))
                {
                    value += pair.Value;
                    if (isMultipliers)
                    {
                        value -= 1.0;
                    }
                }
                else
                {
                    value = pair.Value;
                }

                toDictionary[pair.Key] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ValidateIsNotReadOnly()
        {
            if (this.IsReadOnly)
            {
                throw new Exception("The stats cache has been made read-only.");
            }
        }
    }
}