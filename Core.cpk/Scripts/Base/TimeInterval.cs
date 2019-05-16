namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    [Serializable]
    public readonly struct TimeInterval : IRemoteCallParameter, IEquatable<TimeInterval>
    {
        public TimeInterval(double fromHour, double toHour)
        {
            if (fromHour < 0
                || toHour < 0)
            {
                fromHour += 24;
                toHour += 24;
            }

            this.FromHour = fromHour;
            this.ToHourNormalized = toHour;

            if (this.FromHour > this.ToHourNormalized)
            {
                this.ToHourNormalized += 24;
            }

            this.DurationHours = this.ToHourNormalized - this.FromHour;
        }

        public double DurationHours { get; }

        public double FromHour { get; }

        /// <summary>
        /// Normalized ToHour. It means if we have range from 21:00 to 2:00 then normalized ToHour will be 26:00.
        /// </summary>
        public double ToHourNormalized { get; }

        public bool Equals(TimeInterval other)
        {
            return this.FromHour.Equals(other.FromHour)
                   && this.DurationHours.Equals(other.DurationHours);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TimeInterval other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.FromHour.GetHashCode() * 397) ^ this.DurationHours.GetHashCode();
            }
        }

        public double SharedCalculateHoursRemainsToEnd(double timeHours)
        {
            this.NormalizeTime(ref timeHours,
                               out var fromHour,
                               out var toHour);

            var timeToStart = fromHour - timeHours;
            var timeToEnd = toHour - timeHours;
            if (timeToStart < 0
                && timeToEnd > 0)
            {
                return timeToEnd;
            }

            return 0;
        }

        public double SharedCalculateHoursRemainsToStart(double timeHours)
        {
            this.NormalizeTime(ref timeHours,
                               out var fromHour,
                               out var toHour);

            var timeToStart = fromHour - timeHours;
            var timeToEnd = toHour - timeHours;
            if (timeToStart < 0
                && timeToEnd > 0)
            {
                // already in time interval
                return 0;
            }

            if (timeToStart > 0)
            {
                return timeToStart;
            }

            return timeToStart + 24;
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public override string ToString()
        {
            return string.Format("From {0:0.##}h to {1:0.##}h (duration {2:0.##}h)",
                                 this.FromHour,
                                 this.ToHourNormalized,
                                 this.DurationHours);
        }

        private void NormalizeTime(ref double timeHours, out double fromHour, out double toHour)
        {
            timeHours %= 24;
            if (timeHours < 0)
            {
                timeHours += 24;
            }

            fromHour = this.FromHour;
            toHour = this.ToHourNormalized;
            if (timeHours >= fromHour)
            {
                return;
            }

            timeHours += 24;
            fromHour += 24;
            toHour += 24;
        }
    }
}