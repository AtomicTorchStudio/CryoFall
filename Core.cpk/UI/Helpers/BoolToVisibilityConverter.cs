namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ReSharper disable once PossibleNullReferenceException
            var valueAsBool = (bool)value;
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            if (parameter as string == "Reverse")
            {
                valueAsBool = !valueAsBool;
            }

            return valueAsBool ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}