namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ReSharper disable once PossibleNullReferenceException
            var valueAsNumber = System.Convert.ToInt64(value);

            var isVisible = valueAsNumber > 0;
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            if (parameter as string == "Reverse")
            {
                isVisible = !isVisible;
            }

            return isVisible
                       ? Visibility.Visible
                       : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}