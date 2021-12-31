namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ViewModelPersonalStatistics : BaseViewModel
    {
        public ViewModelPersonalStatistics()
        {
            var technologies = ClientCurrentCharacterHelper.PrivateState.Technologies;
            var statistics = ClientCurrentCharacterHelper.PrivateState.Statistics;
            var isPvP = !PveSystem.ServerIsPvE;

            var entries = new List<IViewModelPersonalStatisticsEntry>();

            entries.Add(Create(CoreStrings.StatisticsTotalLearningPoints,
                               technologies,
                               _ => _.LearningPointsAccumulatedTotal));

            entries.Add(Create(CoreStrings.StatisticsMineralsMined,
                               statistics,
                               _ => _.MineralsMined));

            entries.Add(Create(CoreStrings.StatisticsTreesCut,
                               statistics,
                               _ => _.TreesCut));

            entries.Add(Create(CoreStrings.StatisticsFarmPlantsHarvested,
                               statistics,
                               _ => _.FarmPlantsHarvested));

            if (!isPvP) // in case of PvP it's added later
            {
                entries.Add(Create(CoreStrings.StatisticsDeaths,
                                   statistics,
                                   _ => _.Deaths));
            }

            if (isPvP)
            {
                entries.Add(
                    Create(CoreStrings.StatisticsPvpScore,
                           statistics,
                           _ => _.PvpScore));

                entries.Add(
                    Create(CoreStrings.StatisticsPvpKills,
                           statistics,
                           _ => _.PvpKills));

                entries.Add(Create(CoreStrings.StatisticsDeaths,
                                   statistics,
                                   _ => _.Deaths));

                // KD is more complex as it's not a SyncToClient property but computed on the client side
                // from kills and deaths number.
                entries.Add(
                    new ViewModelPersonalStatisticsEntryKillDeathRatio(statistics));
            }

            this.Entries = entries;
        }

        public interface IViewModelPersonalStatisticsEntry
        {
            string Name { get; }

            string ValueText { get; }
        }

        public IReadOnlyList<IViewModelPersonalStatisticsEntry> Entries { get; }

        private static IViewModelPersonalStatisticsEntry Create<TKey, TValue>(
            string name,
            TKey eventSource,
            Expression<Func<TKey, TValue>> propertyGetFunc)
            where TKey : INetObject
        {
            return new ViewModelPersonalStatisticsEntry<TKey, TValue>(
                name,
                eventSource,
                propertyGetFunc);
        }

        public class ViewModelPersonalStatisticsEntry<TKey, TValue> : BaseViewModel, IViewModelPersonalStatisticsEntry
            where TKey : INetObject
        {
            private readonly Func<TKey, TValue> compiledGetter;

            private readonly TKey eventSource;

            public ViewModelPersonalStatisticsEntry(
                string name,
                TKey eventSource,
                Expression<Func<TKey, TValue>> propertyGetFunc)
            {
                this.eventSource = eventSource;
                this.compiledGetter = propertyGetFunc.Compile();
                this.Name = name;
                eventSource.ClientSubscribe(propertyGetFunc,
                                            () => this.NotifyPropertyChanged(nameof(this.ValueText)),
                                            this);
            }

            public string Name { get; }

            public string ValueText
            {
                get
                {
                    var value = this.compiledGetter.Invoke(this.eventSource);
                    return value switch
                    {
                        double valueDouble => valueDouble.ToString("F2"),
                        float valueFloat   => valueFloat.ToString("F2"),
                        _                  => value?.ToString()
                    };
                }
            }
        }

        public class ViewModelPersonalStatisticsEntryKillDeathRatio : BaseViewModel, IViewModelPersonalStatisticsEntry
        {
            private readonly PlayerCharacterStatistics statistics;

            public ViewModelPersonalStatisticsEntryKillDeathRatio(PlayerCharacterStatistics statistics)
            {
                this.statistics = statistics;
                statistics.ClientSubscribe(_ => _.PvpKills,
                                           _ => this.NotifyPropertyChanged(nameof(this.ValueText)),
                                           this);

                statistics.ClientSubscribe(_ => _.Deaths,
                                           _ => this.NotifyPropertyChanged(nameof(this.ValueText)),
                                           this);
            }

            public string Name => CoreStrings.StatisticsPvpKillDeathRatio;

            public string ValueText => this.statistics.PvpKillDeathRatio.ToString("F2");
        }
    }
}