namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Data;

    public abstract class BaseStatsDictionary : IReadOnlyStatsDictionary
    {
        protected readonly Dictionary<StatName, double> Multipliers = new Dictionary<StatName, double>();

        protected readonly Dictionary<StatName, double> Values = new Dictionary<StatName, double>();

        /// <summary>
        /// Set special mode - sum of multipliers instead of multiplication.
        /// </summary>
        protected bool IsMultipliersSummed;

        private StatsSources sources;

        public bool IsEmpty => this.Values.Count == 0
                               && this.Multipliers.Count == 0;

        public bool IsReadOnly { get; private set; }

        public StatsSources Sources => this.sources;

        IReadOnlyDictionary<StatName, double> IReadOnlyStatsDictionary.Multipliers => this.Multipliers;

        IReadOnlyDictionary<StatName, double> IReadOnlyStatsDictionary.Values => this.Values;

        public void AddPercent(IProtoEntity source, StatName statName, double percent)
        {
            if (percent == 0d)
            {
                return;
            }

            var multiplier = percent / 100d;

            if (this.IsMultipliersSummed)
            {
                // simply sum multipliers (like values)
                if (multiplier == 0)
                {
                    return;
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
            }
            else
            {
                if (multiplier <= -1)
                {
                    // -100% or lower - wow!
                    StatsSources.RegisterPercent(ref this.sources, source, statName, -100);
                    this.Multipliers[statName] = 0;
                    return;
                }

                StatsSources.RegisterPercent(ref this.sources, source, statName, percent);

                multiplier += 1;

                if (this.Multipliers.TryGetValue(statName, out var currentMultiplier))
                {
                    // Simply multiply multipliers.
                    // We're doing this to avoid case when -50% + -50% = -100%
                    // as it could broke the game in some cases (like movement speed).
                    // But if percent is provided as -100%, then it will be converted to multiplier==0
                    // so we still can reset some stats to 0 by using percent effects.

                    if (currentMultiplier == 0d)
                    {
                        // early return - the final multiplier also will be 0 and nothing changes
                        return;
                    }

                    multiplier *= currentMultiplier;
                }
            }

            this.ValidateIsNotReadOnly();
            this.Multipliers[statName] = multiplier;
        }

        public void AddValue(IProtoEntity source, StatName statName, double value)
        {
            if (value == 0d)
            {
                return;
            }

            StatsSources.RegisterValue(ref this.sources, source, statName, value);

            if (this.Values.TryGetValue(statName, out var currentValue))
            {
                // simply sum with existing value
                value += currentValue;
            }

            this.ValidateIsNotReadOnly();
            this.Values[statName] = value;
        }

        public FinalStatsCache CalculateFinalStatsCache()
        {
            var finalValues = new Dictionary<StatName, double>(
                capacity: Math.Max(this.Values.Count, this.Multipliers.Count));

            var finalMultipliers =
                new Dictionary<StatName, double>(this.Multipliers);

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

        public void Merge(IReadOnlyStatsDictionary otherStatsCache)
        {
            this.ValidateIsNotReadOnly();

            // values are merged via sum
            this.Merge(
                this.Values,
                otherStatsCache.Values,
                isMultiplied: false);

            // multipliers are merged via multiplication
            this.Merge(
                this.Multipliers,
                otherStatsCache.Multipliers,
                isMultiplied: !this.IsMultipliersSummed);

            StatsSources.Merge(ref this.sources, otherStatsCache.Sources);
        }

        protected virtual void MakeReadOnly()
        {
            this.IsReadOnly = true;
        }

        protected void Merge(
            IDictionary<StatName, double> toDictionary,
            IReadOnlyDictionary<StatName, double> fromDictionary,
            bool isMultiplied)
        {
            foreach (var pair in fromDictionary)
            {
                if (toDictionary.TryGetValue(pair.Key, out var value))
                {
                    if (isMultiplied)
                    {
                        value *= pair.Value;
                    }
                    else
                    {
                        value += pair.Value;
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