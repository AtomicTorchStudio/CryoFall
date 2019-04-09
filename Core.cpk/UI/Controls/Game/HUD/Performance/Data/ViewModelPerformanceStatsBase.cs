namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public abstract class ViewModelPerformanceStatsBase : BaseViewModel
    {
        private PerformanceMetricSeverityLevel indicatorSeverity = PerformanceMetricSeverityLevel.Green;

        public virtual event Action IndicatorSeverityChanged;

        public bool AreExtraPanelsVisible { get; private set; }

        public PerformanceMetricSeverityLevel IndicatorSeverity
        {
            get => this.indicatorSeverity;
            protected set
            {
                if (this.indicatorSeverity == value)
                {
                    return;
                }

                this.indicatorSeverity = value;
                this.IndicatorSeverityChanged?.Invoke();
                this.NotifyThisPropertyChanged();

                this.IsAllGoodPanelVisible = this.indicatorSeverity == PerformanceMetricSeverityLevel.Green;
                this.IsSuggestionsPanelVisible = this.indicatorSeverity > PerformanceMetricSeverityLevel.Green;

                this.AreExtraPanelsVisible = this.IsAllGoodPanelVisible
                                             || this.IsSuggestionsPanelVisible;
            }
        }

        public bool IsAllGoodPanelVisible { get; private set; }

        public bool IsSuggestionsPanelVisible { get; private set; }

        protected static int SecondsToMs(double seconds)
        {
            return (int)Math.Round(seconds * 1000,
                                   MidpointRounding.AwayFromZero);
        }
    }
}