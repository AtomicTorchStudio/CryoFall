namespace AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    [Serializable]
    public readonly struct GameTimeInterval : IRemoteCallParameter, IEquatable<GameTimeInterval>
    {
        public GameTimeInterval(double fromHour, double toHour)
        {
            if (fromHour < 0
                || fromHour > TimeOfDaySystem.GameDayHoursCount)
            {
                throw new ArgumentOutOfRangeException(nameof(fromHour));
            }

            if (toHour < 0
                || toHour > TimeOfDaySystem.GameDayHoursCount)
            {
                throw new ArgumentOutOfRangeException(nameof(toHour));
            }

            this.FromHour = fromHour;
            this.ToHourNormalized = toHour;

            if (this.FromHour > this.ToHourNormalized)
            {
                this.ToHourNormalized += TimeOfDaySystem.GameDayHoursCount;
            }

            this.DurationHours = this.ToHourNormalized - this.FromHour;
        }

        public double DurationHours { get; }

        public double FromHour { get; }

        /// <summary>
        /// Normalized ToHour. It means if we have range from 21:00 to 2:00 then normalized ToHour will be 26:00.
        /// </summary>
        public double ToHourNormalized { get; }

        /// <summary>
        /// Calculates linear fraction.
        /// If the hour is in the middle of time interval, it will return 1, if not in the interval - 0.
        /// Otherwise something in between.
        /// </summary>
        public double CalculateCurrentFraction(double timeHours)
        {
            if (timeHours < this.FromHour)
            {
                timeHours += TimeOfDaySystem.GameDayHoursCount;
            }

            var delta = (timeHours - this.FromHour) / this.DurationHours;
            if (delta < 0
                || delta > 1)
            {
                // out of range
                return 0;
            }

            // We have delta value in 0-1 range where 1 is the end of the duration.
            // But we must return result in 0-1 range where 1 is the medium of the range.
            var result = delta * 2;
            if (result <= 1)
            {
                return result;
            }

            return 2 - result;
        }

        public bool Equals(GameTimeInterval other)
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

            return obj is GameTimeInterval other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.FromHour.GetHashCode() * 397) ^ this.DurationHours.GetHashCode();
            }
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public override string ToString()
        {
            return string.Format("From {0:F2} to {1:F2} (duration {2:F2}h)",
                                 this.FromHour,
                                 this.ToHourNormalized,
                                 this.DurationHours);
        }
    }
}