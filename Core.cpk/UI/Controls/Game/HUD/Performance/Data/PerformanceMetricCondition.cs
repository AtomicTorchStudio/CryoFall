namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance.Data
{
    using System;

    public class PerformanceMetricCondition
    {
        public readonly string Description;

        public readonly PerformanceMetricSeverityLevel IndicatorSeverity;

        public readonly PerformanceMetricSeverityLevel MetricSeverity;

        public readonly Func<int, bool> Test;

        public PerformanceMetricCondition(
            Func<int, bool> test,
            PerformanceMetricSeverityLevel metricSeverity,
            PerformanceMetricSeverityLevel indicatorSeverity,
            string description)
        {
            this.Test = test;
            this.MetricSeverity = metricSeverity;
            this.IndicatorSeverity = indicatorSeverity;
            this.Description = description;
        }
    }
}