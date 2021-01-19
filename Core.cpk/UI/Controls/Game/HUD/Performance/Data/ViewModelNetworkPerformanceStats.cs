namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using static PerformanceMetricSeverityLevel;

    public class ViewModelNetworkPerformanceStats : ViewModelPerformanceStatsBase
    {
        public const int PingSevereValue = 250;

        public const int PingSubstantialValue = 125;

        private const int PacketLossSeverePercent = 15;

        private const int PacketLossSubstantialPercent = 5;

        private static readonly ICurrentGameService Game = Api.Client.CurrentGame;

        public ViewModelNetworkPerformanceStats()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.RefreshCallback();
        }

        public ViewModelPerformanceMetric PacketLoss { get; }
            = new(CoreStrings.Network_PacketLoss,
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                "%",
                new PerformanceMetricCondition(
                    range => range > PacketLossSeverePercent,
                    metricSeverity: Red,
                    indicatorSeverity: Red,
                    description:
                    CoreStrings.Network_PacketLoss_Description),
                new PerformanceMetricCondition(
                    range => range > PacketLossSubstantialPercent,
                    metricSeverity: Yellow,
                    indicatorSeverity: Yellow,
                    description:
                    CoreStrings.Network_PacketLoss_Description));

        public ViewModelPerformanceMetric PingAverage { get; }
            = new(CoreStrings.Network_PingAverage,
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                "ms",
                new PerformanceMetricCondition(
                    ping => ping > PingSevereValue,
                    metricSeverity: Red,
                    indicatorSeverity: Red,
                    description:
                    CoreStrings.Network_PingAverage_SeverityRed),
                new PerformanceMetricCondition(
                    ping => ping > PingSubstantialValue,
                    metricSeverity: Yellow,
                    indicatorSeverity: Yellow,
                    description:
                    CoreStrings.Network_PingAverage_SeverityYellow));

        public ViewModelPerformanceMetric PingFluctuationRange { get; }
            = new(CoreStrings.Network_PingFluctuationRange,
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                "ms",
                new PerformanceMetricCondition(
                    range => range > 100,
                    metricSeverity: Red,
                    indicatorSeverity: Yellow,
                    description:
                    CoreStrings.Network_PingFluctuationRange_SeverityRed),
                new PerformanceMetricCondition(
                    range => range > 50,
                    metricSeverity: Yellow,
                    indicatorSeverity: None,
                    description:
                    CoreStrings.Network_PingFluctuationRange_SeverityYellow));

        public ViewModelPerformanceMetric PingGame { get; }
            = new(CoreStrings.Network_PingGame,
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                "ms",
                new PerformanceMetricCondition(
                    ping => ping > PingSevereValue,
                    metricSeverity: Red,
                    indicatorSeverity: Red,
                    description: null), // nothing here as we're using PingAverage as the primary metric of ping quality
                new PerformanceMetricCondition(
                    ping => ping > PingSubstantialValue,
                    metricSeverity: Yellow,
                    indicatorSeverity: Yellow,
                    description: null));

        public ViewModelPerformanceMetric PingJitter { get; }
            = new(CoreStrings.Network_Jitter,
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                "ms",
                new PerformanceMetricCondition(
                    jitter => jitter > 50,
                    metricSeverity: Red,
                    indicatorSeverity: Yellow,
                    description: CoreStrings.Network_Jitter_SeverityRed),
                new PerformanceMetricCondition(
                    jitter => jitter > 25,
                    metricSeverity: Yellow,
                    indicatorSeverity: None,
                    description: CoreStrings.Network_Jitter_SeverityYellow));

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
                ClientTimersSystem.AddAction(delaySeconds: ViewModelPerformanceMetric.RefreshInterval,
                                             this.RefreshCallback);
            }
        }

        private void RefreshMetrics()
        {
            if (Game.ConnectionState != ConnectionState.Connected
                || LoadingSplashScreenManager.Instance.CurrentState != LoadingSplashScreenState.Hidden)
            {
                // don't measure performance when not connected or the loading splash screen is visible
                this.PingAverage.Reset();
                this.PingGame.Reset();
                this.PingJitter.Reset();
                this.PingFluctuationRange.Reset();
                this.PacketLoss.Reset();
                this.PacketLoss.IsValueAvailable = false;
                return;
            }

            var pingAverageMs = SecondsToMs(Game.GetPingAverageSeconds(yesIKnowIShouldUsePingGameInstead: true));
            this.PingAverage.Update(pingAverageMs);

            var pingGameMs = SecondsToMs(Game.PingGameSeconds);
            this.PingGame.Update(pingGameMs);

            var pingJitterMs = SecondsToMs(Game.PingJitterSeconds);
            this.PingJitter.Update(pingJitterMs);

            var pingFluctuationRangeMs = SecondsToMs(Game.PingFluctuationRangeSeconds);
            this.PingFluctuationRange.Update(pingFluctuationRangeMs);

            var packetLossPercent = Game.PacketLossPercent;
            this.PacketLoss.Update((int)Math.Round(packetLossPercent ?? 0, MidpointRounding.AwayFromZero));
            this.PacketLoss.IsValueAvailable = packetLossPercent.HasValue;

            var maxIndicatorSeverity =
                (PerformanceMetricSeverityLevel)
                Math.Max(
                    (byte)this.PingAverage.IndicatorSeverity,
                    Math.Max(
                        Math.Max(
                            (byte)this.PingGame.IndicatorSeverity,
                            (byte)this.PingJitter.IndicatorSeverity),
                        Math.Max(
                            (byte)this.PingFluctuationRange.IndicatorSeverity,
                            (byte)this.PacketLoss.IndicatorSeverity)));

            this.IndicatorSeverity = maxIndicatorSeverity;
        }
    }
}