namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public readonly struct ElectricityThresholdsPreset : IRemoteCallParameter, IEquatable<ElectricityThresholdsPreset>
    {
        public static readonly IEqualityComparer<ElectricityThresholdsPreset> ElectricityThresholdsPresetComparer
            = new ElectricityThresholdsPresetEqualityComparer();

        public readonly byte ShutdownPercent;

        public readonly byte StartupPercent;

        public ElectricityThresholdsPreset(byte startupPercent, byte shutdownPercent)
        {
            this.StartupPercent = Math.Min(startupPercent,   (byte)100);
            this.ShutdownPercent = Math.Min(shutdownPercent, (byte)100);
        }

        public bool IsInvalid => this.Equals(default);

        public static string FormatShutdownThreshold(bool isGenerator, byte percent)
        {
            var sb = new StringBuilder();
            if (isGenerator)
            {
                sb.Append(percent == 100 ? "=" : "\u2265"); // >= sign
            }
            else
            {
                sb.Append(percent == 0 ? "=" : "<");
            }

            return sb.Append(percent)
                     .Append("%")
                     .ToString();
        }

        public static string FormatStartupThreshold(bool isGenerator, byte percent)
        {
            var sb = new StringBuilder();
            if (isGenerator)
            {
                sb.Append(percent == 0 ? "=" : "<");
            }
            else
            {
                sb.Append(percent == 100 ? "=" : "\u2265"); // >= sign
            }

            return sb.Append(percent)
                     .Append("%")
                     .ToString();
        }

        public bool Equals(ElectricityThresholdsPreset other)
        {
            return this.StartupPercent == other.StartupPercent
                   && this.ShutdownPercent == other.ShutdownPercent;
        }

        public override bool Equals(object obj)
        {
            return obj is ElectricityThresholdsPreset other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.StartupPercent.GetHashCode() * 397) ^ this.ShutdownPercent.GetHashCode();
            }
        }

        public ElectricityThresholdsPreset Normalize(bool isProducer)
        {
            return isProducer
                       ? NormalizeProducerElectricityThresholds(this)
                       : NormalizeConsumerElectricityThresholds(this);
        }

        public override string ToString()
        {
            return $"Startup: {this.StartupPercent}%, Shutdown: {this.ShutdownPercent}%";
        }

        private static ElectricityThresholdsPreset NormalizeConsumerElectricityThresholds(
            ElectricityThresholdsPreset preset)
        {
            // startup should be > than shutdown
            var startupPercent = Math.Min(preset.StartupPercent,   (byte)100);
            var shutdownPercent = Math.Min(preset.ShutdownPercent, (byte)100);

            if (shutdownPercent == 100)
            {
                shutdownPercent = 99;
                startupPercent = 100;
                return new ElectricityThresholdsPreset(startupPercent, shutdownPercent);
            }

            if (startupPercent <= 1)
            {
                shutdownPercent = 0;
                startupPercent = 1;
                return new ElectricityThresholdsPreset(startupPercent, shutdownPercent);
            }

            if (startupPercent <= shutdownPercent)
            {
                startupPercent = shutdownPercent;
                shutdownPercent--;
            }

            return new ElectricityThresholdsPreset(startupPercent, shutdownPercent);
        }

        private static ElectricityThresholdsPreset NormalizeProducerElectricityThresholds(
            ElectricityThresholdsPreset preset)
        {
            // it's a vice versa version of the consumer preset - startup should be < than shutdown
            // instead of copy-pasting the code just reverse the values, normalize then, and reverse back
            var reversed = new ElectricityThresholdsPreset(startupPercent: preset.ShutdownPercent,
                                                           shutdownPercent: preset.StartupPercent);
            reversed = NormalizeConsumerElectricityThresholds(reversed);
            return new ElectricityThresholdsPreset(startupPercent: reversed.ShutdownPercent,
                                                   shutdownPercent: reversed.StartupPercent);
        }

        private sealed class ElectricityThresholdsPresetEqualityComparer
            : IEqualityComparer<ElectricityThresholdsPreset>
        {
            public bool Equals(ElectricityThresholdsPreset x, ElectricityThresholdsPreset y)
            {
                return x.StartupPercent == y.StartupPercent
                       && x.ShutdownPercent == y.ShutdownPercent;
            }

            public int GetHashCode(ElectricityThresholdsPreset obj)
            {
                unchecked
                {
                    return (obj.StartupPercent.GetHashCode() * 397) ^ obj.ShutdownPercent.GetHashCode();
                }
            }
        }
    }
}