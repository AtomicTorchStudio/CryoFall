namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using static PerformanceMetricSeverityLevel;

    public class ViewModelPerformanceMetric : BaseViewModel
    {
        public const double RefreshInterval = 0.5;

        // We don't want to blink the indicator too often
        // so we will use a delay to ensure that the indicator cannot 
        // change its state if the issue is not sustainable.
        private const int IndicatorSeverityChangeDelaySeconds = 12;

        private const int ValueBufferTotalDurationSeconds = 8;

        private static readonly SolidColorBrush BrushGreen
            = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xE0, 0x00));

        private static readonly SolidColorBrush BrushRed
            = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x22, 0x22));

        private static readonly SolidColorBrush BrushYellow
            = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xEE, 0x44));

        private readonly PerformanceMetricCondition[] metrics;

        private readonly CycledArrayStorage<int> valuesBuffer
            = new CycledArrayStorage<int>((uint)(ValueBufferTotalDurationSeconds / RefreshInterval));

        private PerformanceMetricSeverityLevel indicatorSeverity;

        private double indicatorSeverityDelayStartedTime;

        private PerformanceMetricSeverityLevel metricSeverity;

        public ViewModelPerformanceMetric(
            string title,
            string unitsText,
            params PerformanceMetricCondition[] metrics)
        {
            this.Title = title + ": ";
            this.UnitsText = string.IsNullOrEmpty(unitsText) ? null : " " + unitsText;
            this.metrics = metrics;
            this.Reset();
        }

        public PerformanceMetricSeverityLevel IndicatorSeverity
        {
            get => this.indicatorSeverity;
            private set
            {
                var time = Client.Core.ClientRealTime;
                if (this.indicatorSeverity == value)
                {
                    // no actual change
                    this.indicatorSeverityDelayStartedTime = time;
                    return;
                }

                // the change is required
                if (time - this.indicatorSeverityDelayStartedTime
                    < IndicatorSeverityChangeDelaySeconds)
                {
                    // the delay is not expired yet
                    // (we don't want to blink the indicator too quickly if the issue is not sustainable)
                    return;
                }

                this.indicatorSeverityDelayStartedTime = time;
                this.indicatorSeverity = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public string IssueDescription { get; private set; }

        public SolidColorBrush MetricBrush => GetBrush(this.MetricSeverity);

        public PerformanceMetricSeverityLevel MetricSeverity
        {
            get => this.metricSeverity;
            private set
            {
                if (this.metricSeverity == value)
                {
                    return;
                }

                this.metricSeverity = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.MetricBrush));
            }
        }

        public string Title { get; }

        public string UnitsText { get; }

        public int Value { get; private set; } = int.MaxValue;

        public void Reset()
        {
            this.valuesBuffer.Clear();
            this.Value = 0;
            this.MetricSeverity = None;

            this.indicatorSeverity = None;
            this.indicatorSeverityDelayStartedTime = Client.Core.ClientRealTime;
            this.NotifyPropertyChanged(nameof(this.IndicatorSeverity));
        }

        public void Update(int value)
        {
            this.Value = value;
            if (this.metrics?.Length == 0)
            {
                return;
            }

            this.valuesBuffer.Add(value);

            // match metric by the actual value
            var matchedMetric = this.MatchMetric(value);
            this.MetricSeverity = matchedMetric?.MetricSeverity ?? Green;
            this.IssueDescription = matchedMetric?.Description;

            // match metric by the average value
            if (this.valuesBuffer.Count < this.valuesBuffer.Capacity)
            {
                // not enough measurements
                this.IndicatorSeverity = None;
                return;
            }

            // enough measurements
            var valueAverage = (int)Math.Round(this.valuesBuffer.CalculateAverage(),
                                               MidpointRounding.AwayFromZero);
            var matchedAverage = this.MatchMetric(valueAverage);
            this.IndicatorSeverity = matchedAverage?.IndicatorSeverity ?? Green;
        }

        private static SolidColorBrush GetBrush(PerformanceMetricSeverityLevel performanceMetricSeverityLevel)
        {
            switch (performanceMetricSeverityLevel)
            {
                case None:
                case Green:
                    return BrushGreen;

                case Yellow:
                    return BrushYellow;

                case Red:
                    return BrushRed;

                default:
                    throw new ArgumentOutOfRangeException(nameof(performanceMetricSeverityLevel),
                                                          performanceMetricSeverityLevel,
                                                          null);
            }
        }

        private PerformanceMetricCondition MatchMetric(int value)
        {
            PerformanceMetricCondition matched = null;
            // ReSharper disable once PossibleNullReferenceException
            foreach (var metric in this.metrics)
            {
                if (!metric.Test(value))
                {
                    continue;
                }

                matched = metric;
                break;
            }

            return matched;
        }
    }
}