namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = string.IsNullOrEmpty(value as string);
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            if (parameter as string == "Reverse")
            {
                visible = !visible;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}