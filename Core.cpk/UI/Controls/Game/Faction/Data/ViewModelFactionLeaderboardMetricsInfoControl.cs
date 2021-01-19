namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi;

    public class ViewModelFactionLeaderboardMetricsInfoControl : BaseViewModel
    {
        public IReadOnlyList<DataEntryFactionScoreMetricValue> Entries { get; private set; }

        public bool IsLoading { get; private set; } = true;

        public void SetData(IReadOnlyDictionary<ProtoFactionScoreMetric, uint> scoreMetrics)
        {
            var entries = scoreMetrics?.Where(p => p.Value > 0)
                                      .OrderBy(p => p.Key.Name)
                                      .Select(p => new DataEntryFactionScoreMetricValue(p.Key, p.Value))
                                      .ToArray();
            this.Entries = entries?.Length > 0 ? entries : null;
            this.IsLoading = false;
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

            public string Name => this.protoFactionScoreMetric.Name;

            public uint Value { get; }

            /*public string Text => string.Format("{0}: [b]{1}[/b]",
                                                this.protoFactionScoreMetric.Name,
                                                this.value);*/
        }
    }
}