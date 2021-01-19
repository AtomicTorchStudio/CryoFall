namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.GameEngine.Common.Extensions;

    /// <summary>
    /// Provides key-value storage of calculated character stats.
    /// </summary>
    public readonly struct FinalStatsCache
    {
        public static readonly FinalStatsCache Empty
            = new(new Dictionary<StatName, double>(),
                  new Dictionary<StatName, double>(),
                  new StatsSources());

        public readonly StatsSources Sources;

        private readonly IDictionary<StatName, double> dictionaryFinalValues;

        private readonly IDictionary<StatName, double> dictionaryMultipliers;

        private readonly bool isClean;

        public FinalStatsCache(
            IDictionary<StatName, double> dictionaryFinalValues,
            IDictionary<StatName, double> dictionaryMultipliers,
            StatsSources sources)
        {
            this.dictionaryFinalValues = dictionaryFinalValues;
            this.dictionaryMultipliers = dictionaryMultipliers;
            this.Sources = sources;
            this.isClean = true;
        }

        private FinalStatsCache(
            IDictionary<StatName, double> dictionaryFinalValues,
            IDictionary<StatName, double> dictionaryMultipliers,
            StatsSources sources,
            bool isClean)
            : this(dictionaryFinalValues, dictionaryMultipliers, sources)
        {
            this.isClean = isClean;
        }

        public bool IsDirty => !this.isClean;

        public double this[StatName key]
        {
            get
            {
                try
                {
                    return this.dictionaryFinalValues.Find(key);
                }
                catch (NullReferenceException)
                {
                    throw new Exception("Final stats cache is not initialized");
                }
            }
        }

        /// <summary>
        /// Please don't call this directly - it will NOT change the state to dirty, it will return a new FinalStatsCache instance.
        /// </summary>
        public FinalStatsCache AsDirty()
        {
            return new(
                this.dictionaryFinalValues,
                this.dictionaryMultipliers,
                this.Sources,
                isClean: false);
        }

        public bool CustomEquals(in FinalStatsCache other)
        {
            return ReferenceEquals(this.dictionaryFinalValues,    other.dictionaryFinalValues)
                   && ReferenceEquals(this.dictionaryMultipliers, other.dictionaryMultipliers);
        }

        public IEnumerable<KeyValuePair<StatName, double>> EnumerateFinalValues()
        {
            return this.dictionaryFinalValues
                   ?? throw new Exception("Final stats cache is not initialized");
        }

        public double GetMultiplier(StatName key)
        {
            try
            {
                return this.dictionaryMultipliers.TryGetValue(key, out var multiplier)
                           ? multiplier
                           : 1;
            }
            catch (NullReferenceException)
            {
                throw new Exception("Final stats cache is not initialized");
            }
        }

        public bool HasPerk(StatName statNamePerk)
        {
            return this[statNamePerk] >= 1;
        }
    }
}