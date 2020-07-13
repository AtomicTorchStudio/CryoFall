namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    [Serializable]
    public readonly struct DayTimeInterval : IRemoteCallParameter, IEquatable<DayTimeInterval>
    {
        public DayTimeInterval(double fromHour, double toHour)
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

        public bool Equals(DayTimeInterval other)
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

            return obj is DayTimeInterval other && this.Equals(other);
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
            this.CalculateRange(ref timeHours,
                                out var timeToStart,
                                out var timeToEnd);

            if (timeToStart < 0
                && timeToEnd > 0)
            {
                return timeToEnd;
            }

            return 0;
        }

        public double SharedCalculateHoursRemainsToStart(double timeHours)
        {
            this.CalculateRange(ref timeHours,
                                out var timeToStart,
                                out var timeToEnd);

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

        private void CalculateRange(
            ref double timeHours,
            out double timeToStart,
            out double timeToEnd)
        {
            timeHours %= 24;
            if (timeHours < 0)
            {
                timeHours += 24;
            }

            // normalize time
            var fromHour = this.FromHour;
            var toHour = this.ToHourNormalized;
            if (timeHours >= fromHour)
            {
                // timeHours is within the range
            }
            else if (toHour >= 24)
            {
                // timeHours is outside the range
                // and toHour is overlapping the full 24 hours day
                // get "range after midnight"
                fromHour -= 24;
                toHour -= 24;
            }

            // calculate time range
            timeToStart = fromHour - timeHours;
            timeToEnd = toHour - timeHours;
        }
    }
}