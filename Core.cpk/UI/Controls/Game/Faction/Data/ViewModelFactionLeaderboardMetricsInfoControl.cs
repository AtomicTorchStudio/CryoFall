namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem;
    using AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi;

    public class ViewModelFactionLeaderboardMetricsInfoControl : BaseViewModel
    {
        public ViewModelFactionLeaderboardMetricsInfoControl()
        {
            this.UpdateTimers();
        }

        public IReadOnlyList<DataEntryFactionScoreMetricValue> Entries { get; private set; }

        public bool IsLoading { get; private set; } = true;

        public string LeaderboardNextUpdateInText
        {
            get
            {
                var timeRemains = FactionLeaderboardSystem.ClientNextLeaderboardUpdateTime
                                  - Client.CurrentGame.ServerFrameTimeRounded;
                if (timeRemains <= 0)
                {
                    return "0";
                }

                return ClientTimeFormatHelper.FormatTimeDuration(timeRemains);
            }
        }

        public string TotalScore { get; private set; }

        public void Refresh()
        {
            this.NotifyPropertyChanged(nameof(this.LeaderboardNextUpdateInText));
        }

        public void Reset()
        {
            this.IsLoading = true;
        }

        public void SetData(IReadOnlyDictionary<ProtoFactionScoreMetric, uint> scoreMetrics, string totalScore)
        {
            var entries = scoreMetrics?.Where(p => p.Value > 0)
                                      .OrderBy(p => p.Key.Name)
                                      .Select(p => new DataEntryFactionScoreMetricValue(p.Key, p.Value))
                                      .ToArray();
            this.Entries = entries?.Length > 0 ? entries : null;
            this.TotalScore = totalScore;
            this.IsLoading = false;
        }

        private void UpdateTimers()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.Refresh();

            // schedule next update
            ClientTimersSystem.AddAction(
                delaySeconds: 1,
                this.UpdateTimers);
        }

        [NotPersistent]
        [NotNetworkAvailable]
        public readonly struct DataEntryFactionScoreMetricValue
        {
            private readonly ProtoFactionScoreMetric protoFactionScoreMetric;

            public DataEntryFactionScoreMetricValue(ProtoFactionScoreMetric protoFactionScoreMetric, uint value)
            {
                this.protoFactionScoreMetric = protoFactionScoreMetric;
                this.Value = value;
            }

            public string Description => this.protoFactionScoreMetric.Description;

            public string Name => this.protoFactionScoreMetric.Name;

            public uint Value { get; }
        }
    }
}