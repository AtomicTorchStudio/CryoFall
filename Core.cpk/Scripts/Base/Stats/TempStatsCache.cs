namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides temporal storage to combine status effects and then calculate the final stats cache.
    /// <see cref="BaseStatsDictionary.CalculateFinalStatsCache" />
    /// </summary>
    public sealed class TempStatsCache : BaseStatsDictionary, IDisposable
    {
        private static readonly Stack<TempStatsCache> Pool = new Stack<TempStatsCache>(capacity: 10);

        /// <summary>
        /// You cannot create it yourself - please use <see cref="GetFromPool" />.
        /// </summary>
        private TempStatsCache()
        {
        }

        /// <summary>
        /// Please dispose after use - apply "using" pattern.
        /// </summary>
        public static TempStatsCache GetFromPool(bool isMultipliersSummed)
        {
            var result = Pool.Count > 0
                             ? Pool.Pop()
                             : new TempStatsCache();

            result.IsMultipliersSummed = isMultipliersSummed;
            return result;
        }

        public void Clear()
        {
            this.ValidateIsNotReadOnly();
            this.Values.Clear();
            this.Multipliers.Clear();
            this.Sources.Clear();
            this.IsMultipliersSummed = false;
        }

        public void Dispose()
        {
            // return to pool
            this.Clear();
            Pool.Push(this);
        }

        public double GetMultiplier(StatName statName)
        {
            return this.Multipliers.TryGetValue(statName, out var result)
                       ? result
                       : 1;
        }

        public double GetValue(StatName statName)
        {
            return this.Values.TryGetValue(statName, out var result)
                       ? result
                       : 0;
        }

        protected override void MakeReadOnly()
        {
            // yeah, we're violating Liskov substitution principle here but it's an acceptable compromise in our case
            throw new Exception(nameof(TempStatsCache) + " cannot be made readonly!");
        }
    }
}