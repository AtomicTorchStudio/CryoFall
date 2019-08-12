namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Home.Data
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class DateToStringConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var date = (DateTime)value;
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            var format = parameter as string ?? "d MMMM yyyy";
            return date.ToLocalTime()
                       .ToString(format, CultureInfo.CurrentUICulture)
                       .ToUpperInvariant();
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}