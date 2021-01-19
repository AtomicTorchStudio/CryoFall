namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.StatModificationDisplay.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using JetBrains.Annotations;

    public class ViewModelStatModificationDisplay : BaseViewModel
    {
        private readonly bool hideDefenseStats;

        private readonly Lazy<IReadOnlyList<StatModificationData>> lazyStatModifications;

        public ViewModelStatModificationDisplay(IReadOnlyStatsDictionary effects, bool hideDefenseStats)
        {
            this.hideDefenseStats = hideDefenseStats;
            this.lazyStatModifications = new Lazy<IReadOnlyList<StatModificationData>>(
                () => this.CreateEffectsList(effects));
        }

        [CanBeNull]
        public IReadOnlyList<StatModificationData> Entries => this.lazyStatModifications.Value;

        private IReadOnlyList<StatModificationData> CreateEffectsList(IReadOnlyStatsDictionary statsDictionary)
        {
            var multipliers = statsDictionary.Multipliers;
            var totalEntriesCount = multipliers.Count + statsDictionary.Values.Count;

            if (totalEntriesCount == 0)
            {
                return null;
            }

            var result = new Dictionary<StatName, StatModificationData>(capacity: totalEntriesCount);

            foreach (var entry in statsDictionary.Values)
            {
                var statName = entry.Key;
                if (!this.IsHidden(statName))
                {
                    AppendValue(statName, entry.Value);
                }
            }

            foreach (var entry in multipliers)
            {
                var statName = entry.Key;
                if (!this.IsHidden(statName))
                {
                    AppendPercent(statName, entry.Value);
                }
            }

            return result.Values.OrderBy(p => p.StatName)
                         .ToList();

            void AppendValue(StatName key, double value)
            {
                if (result.TryGetValue(key, out var entry))
                {
                    entry.Value += value;
                    return;
                }

                result[key] = new StatModificationData(key,
                                                       value,
                                                       percent: 1.0);
            }

            void AppendPercent(StatName key, double percent)
            {
                if (result.TryGetValue(key, out var entry))
                {
                    entry.Percent += percent;
                    return;
                }

                result[key] = new StatModificationData(key,
                                                       value: 0,
                                                       percent);
            }
        }

        private bool IsHidden(StatName statName)
        {
            if (!this.hideDefenseStats)
            {
                return false;
            }

            foreach (var pair in WeaponDamageSystem.DefenseStatNameBinding)
            {
                if (pair.Value == statName)
                {
                    // it's a defense stat
                    return true;
                }
            }

            return false;
        }
    }
}