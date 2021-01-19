namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;

    public static class ClientLocalTimeZoneHelper
    {
        public static TimeSpan CurrentUtcOffset
            => TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

        public static string CurrentUtcOffsetText
        {
            get
            {
                var utcOffset = CurrentUtcOffset;
                var result = utcOffset.ToString("hh\\:mm");
                if (utcOffset.TotalSeconds > 0)
                {
                    result = "+" + result;
                }

                return result;
            }
        }

        public static string GetTextTimeAlreadyConvertedToLocalTimeZone()
        {
            return string.Format(CoreStrings.TimeAlreadyConvertedToLocalTimeZone_Format,
                                 CurrentUtcOffsetText);
        }
    }
}