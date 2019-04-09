namespace AtomicTorch.CBND.CoreMod.Systems.DayNightSystem
{
    using System;

    public struct GameTimeInterval
    {
        public GameTimeInterval(double fromHour, double toHour)
        {
            if (fromHour < 0
                || fromHour > DayNightSystem.GameDayHoursCount)
            {
                throw new ArgumentOutOfRangeException(nameof(fromHour));
            }

            if (toHour < 0
                || toHour > DayNightSystem.GameDayHoursCount)
            {
                throw new ArgumentOutOfRangeException(nameof(toHour));
            }

            this.FromHour = fromHour;
            this.ToHourNormalized = toHour;

            if (this.FromHour > this.ToHourNormalized)
            {
                this.ToHourNormalized += DayNightSystem.GameDayHoursCount;
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
                timeHours += DayNightSystem.GameDayHoursCount;
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
    }
}