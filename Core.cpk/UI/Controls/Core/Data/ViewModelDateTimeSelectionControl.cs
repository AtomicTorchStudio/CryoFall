namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System;
    using System.Globalization;

    public class ViewModelDateTimeSelectionControl : BaseViewModel
    {
        private DateTime dateTime = DateTime.Today
                                            .ToUniversalTime()
                                            .Date
                                            .AddDays(1)
                                            .AddHours(13);

        private bool isAlreadyRefreshing;

        public ViewModelDateTimeSelectionControl()
            : base(isAutoDisposeFields: false)
        {
            this.Refresh();
        }

        // Please note: this value is rebuilt every time the dateTime field changed.
        public EntryWithText[] Days { get; private set; }

        public EntryWithText[] Hours { get; } = CreateNumberRange(0, toExclusive: 24, "00");

        public EntryWithText[] Minutes { get; } = CreateNumberRange(0, toExclusive: 60, "00");

        public EntryWithText[] Months { get; } = CreateMonthsRange();

        public DateTime SelectedDate
        {
            get => this.dateTime;
            set
            {
                if (this.dateTime.Equals(value))
                {
                    return;
                }

                this.dateTime = value;
                this.Refresh();
            }
        }

        public int SelectedDay
        {
            get => this.dateTime.Day;
            set
            {
                this.dateTime = this.dateTime.AddDays(value - this.dateTime.Day);
                this.Refresh();
            }
        }

        public int SelectedHour
        {
            get => this.dateTime.Hour;
            set
            {
                this.dateTime = this.dateTime.AddHours(value - this.dateTime.Hour);
                this.Refresh();
            }
        }

        public int SelectedMinute
        {
            get => this.dateTime.Minute;
            set
            {
                this.dateTime = this.dateTime.AddMinutes(value - this.dateTime.Minute);
                this.Refresh();
            }
        }

        public int SelectedMonth
        {
            get => this.dateTime.Month;
            set
            {
                this.dateTime = this.dateTime.AddMonths(value - this.dateTime.Month);
                this.Refresh();
            }
        }

        public int SelectedYear
        {
            get => this.dateTime.Year;
            set
            {
                this.dateTime = this.dateTime.AddYears(value - this.dateTime.Year);
                this.Refresh();
            }
        }

        public EntryWithText[] Years { get; }
            = CreateNumberRange(DateTime.UtcNow.Year, DateTime.UtcNow.Year + 5, "0000");

        private static EntryWithText[] CreateDayRange(DateTime dateTime, int fromInclusive, int toExclusive)
        {
            var result = new EntryWithText[toExclusive - fromInclusive];
            var index = 0;

            for (var value = fromInclusive; value < toExclusive; value++)
            {
                var date = dateTime.AddDays(value - dateTime.Day);
                result[index] = new EntryWithText(value,
                                                  string.Format("{0:00} ({1})",
                                                                value,
                                                                date.ToString("dddd", CultureInfo.CurrentUICulture)));
                index++;
            }

            return result;
        }

        private static EntryWithText[] CreateMonthsRange()
        {
            var result = new EntryWithText[12];
            var index = 0;

            var dateTime = DateTime.MinValue;

            for (var month = 1; month <= 12; month++)
            {
                var date = dateTime.AddMonths(month - 1);
                result[index] = new EntryWithText(month, date.ToString("MMMM", CultureInfo.CurrentUICulture));
                index++;
            }

            return result;
        }

        private static EntryWithText[] CreateNumberRange(int fromInclusive, int toExclusive, string format)
        {
            var result = new EntryWithText[toExclusive - fromInclusive];
            var index = 0;

            for (var value = fromInclusive; value < toExclusive; value++)
            {
                result[index] = new EntryWithText(value, value.ToString(format));
                index++;
            }

            return result;
        }

        private void Refresh()
        {
            if (this.isAlreadyRefreshing)
            {
                return;
            }

            try
            {
                this.isAlreadyRefreshing = true;

                var daysInMonth = DateTime.DaysInMonth(this.dateTime.Year, this.dateTime.Month);
                if (this.dateTime.Day > daysInMonth)
                {
                    // fix the date
                    this.dateTime = this.dateTime.AddDays(daysInMonth - this.dateTime.Day);
                }

                this.Days = CreateDayRange(this.dateTime, 1, daysInMonth + 1);

                this.NotifyPropertyChanged(nameof(this.SelectedYear));
                this.NotifyPropertyChanged(nameof(this.SelectedMonth));

                this.NotifyPropertyChanged(nameof(this.SelectedDay));
                this.NotifyPropertyChanged(nameof(this.SelectedHour));
                this.NotifyPropertyChanged(nameof(this.SelectedMinute));

                this.NotifyPropertyChanged(nameof(this.SelectedDate));
            }
            finally
            {
                this.isAlreadyRefreshing = false;
            }
        }

        public readonly struct EntryWithText
        {
            public EntryWithText(int value, string text)
            {
                this.Value = value;
                this.Text = string.Intern(text);
            }

            public string Text { get; }

            public int Value { get; }
        }
    }
}