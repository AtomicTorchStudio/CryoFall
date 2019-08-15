namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class ClientTimeFormatHelper
    {
        // suffix for days
        public const string SuffixDays = "d";

        // suffix for hours
        public const string SuffixHours = "h";

        // suffix for minutes
        public const string SuffixMinutes = "m";

        // suffix for seconds
        public const string SuffixSeconds = "s";

        /// <summary>
        /// Format time in seconds to "30d 23h 59m 59s" format.
        /// </summary>
        /// <param name="timeRemainingSeconds"></param>
        /// <returns>Formatted time.</returns>
        public static string FormatTimeDuration(double timeRemainingSeconds, bool roundSeconds = true)
        {
            if (timeRemainingSeconds == double.MaxValue)
            {
                return "...";
            }

            return FormatTimeDuration(time: TimeSpan.FromSeconds(timeRemainingSeconds),
                                      roundSeconds: roundSeconds);
        }

        public static string FormatTimeDuration(
            TimeSpan time,
            bool trimRemainder = false,
            bool roundSeconds = true)
        {
            var sb = new StringBuilder();
            var hasPreviousValue = false;

            if (time.TotalDays >= 2)
            {
                // list days number only if there are at least two days
                TryAppend(time.Days, SuffixDays);

                if (trimRemainder
                    && time.Hours == 0
                    && time.Minutes == 0
                    && time.Seconds == 0)
                {
                    return sb.ToString();
                }

                TryAppend(time.Hours, SuffixHours);
            }
            else
            {
                // list total hours amount (could be < 48)
                TryAppend((int)time.TotalHours, SuffixHours);
            }

            if (trimRemainder
                && time.Minutes == 0
                && time.Seconds == 0)
            {
                return sb.ToString();
            }

            TryAppend(time.Minutes, SuffixMinutes);

            if (trimRemainder
                && time.Seconds == 0)
            {
                return sb.ToString();
            }

            // append seconds
            if (hasPreviousValue)
            {
                sb.Append(' ');
            }

            var seconds = time.TotalSeconds % 60.0;
            sb.Append(hasPreviousValue
                          ? ((int)seconds).ToString("D2") // display seconds as number with leading zero: 01, 02, ... 58, 59
                          : (roundSeconds
                                 ? seconds.ToString("F0") // format without any decimal digits (after the comma)
                                 : seconds.ToString("0.##") // format with up to two decimal digits (after the comma): 0, 0.1, 0.25
                                ))
              .Append(SuffixSeconds);

            return sb.ToString();

            void TryAppend(int value, string suffix)
            {
                if (value <= 0
                    && !hasPreviousValue)
                {
                    // can skip this time entry
                    return;
                }

                // must append this time entry
                if (hasPreviousValue)
                {
                    // add spacing
                    sb.Append(' ');
                    // format as two digits
                    sb.Append(value.ToString("D2"));
                }
                else
                {
                    // for next values spacing will be prepended
                    hasPreviousValue = true;
                    sb.Append(value.ToString());
                }

                sb.Append(suffix);
            }
        }
    }
}