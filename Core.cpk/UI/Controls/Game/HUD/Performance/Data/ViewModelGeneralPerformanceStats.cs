namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ViewModelGeneralPerformanceStats : ViewModelPerformanceStatsBase
    {
        private static readonly IRenderingClientService Rendering = Api.Client.Rendering;

        public ViewModelGeneralPerformanceStats()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.FpsAverage = new ViewModelPerformanceMetric(
                CoreStrings.Performance_FramerateAverage,
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                "FPS",
                new PerformanceMetricCondition(
                    fps => fps < 30,
                    metricSeverity: PerformanceMetricSeverityLevel.Red,
                    indicatorSeverity: PerformanceMetricSeverityLevel.Red,
                    description:
                    CoreStrings.Performance_FPS_SeverityRed),
                new PerformanceMetricCondition(
                    fps => fps < 55,
                    metricSeverity: PerformanceMetricSeverityLevel.Yellow,
                    indicatorSeverity: PerformanceMetricSeverityLevel.Yellow,
                    description:
                    CoreStrings.Performance_FPS_SeverityYellow));

            this.VramUsage = new ViewModelPerformanceMetric(
                CoreStrings.Performance_VRAMUsage,
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                "MB",
                new PerformanceMetricCondition(
                    usageMb => (usageMb - this.VramBudget) > 300,
                    metricSeverity: PerformanceMetricSeverityLevel.Red,
                    indicatorSeverity: PerformanceMetricSeverityLevel.Red,
                    description:
                    CoreStrings.Performance_VRAM_SeverityRed),
                new PerformanceMetricCondition(
                    usageMb => (usageMb - this.VramBudget) > 0,
                    metricSeverity: PerformanceMetricSeverityLevel.Yellow,
                    indicatorSeverity: PerformanceMetricSeverityLevel.Yellow,
                    description:
                    CoreStrings.Performance__VRAM_SeverityYellow));

            this.RefreshCallback();
        }

        public ViewModelPerformanceMetric FpsAverage { get; }

        public int VramBudget { get; private set; }

        public ViewModelPerformanceMetric VramUsage { get; }

        public Visibility VramUsageVisibility { get; private set; }
            = Visibility.Visible;

        private void RefreshCallback()
        {
            if (this.IsDisposed)
            {
                // stop refreshing
                return;
            }

            try
            {
                this.RefreshMetrics();
            }
            finally
            {
                // schedule next refresh
                ClientTimersSystem.AddAction(ViewModelPerformanceMetric.RefreshInterval,
                                             this.RefreshCallback);
            }
        }

        private void RefreshMetrics()
        {
            if (Client.CurrentGame.ConnectionState != ConnectionState.Connected
                || LoadingSplashScreenManager.Instance.CurrentState != LoadingSplashScreenState.Hidden)
            {
                // don't measure performance when not connected or the loading splash screen is visible
                this.FpsAverage.Reset();
                this.VramUsage.Reset();
                return;
            }

            if (!Api.Client.Input.IsGameWindowFocused)
            {
                // don't update performance measurements when the game window is not focused
                return;
            }

            var fpsAverage = Rendering.FpsAverage;
            this.FpsAverage.Update(fpsAverage);

            var vramBudgetMb = (int)(Rendering.GpuVramBudget / (1024 * 1024));
            this.VramBudget = vramBudgetMb;

            var vramUsageMb = (int)(Rendering.GpuVramUsage / (1024 * 1024));
            this.VramUsage.Update(vramUsageMb);
            this.VramUsageVisibility = this.VramUsage.Value > 0
                                           ? Visibility.Visible
                                           : Visibility.Collapsed;

            this.IndicatorSeverity = this.FpsAverage.IndicatorSeverity;
        }
    }
}