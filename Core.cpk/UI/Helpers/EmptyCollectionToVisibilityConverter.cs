namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class EmptyCollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ReSharper disable once PossibleNullReferenceException
            var valueAsEnumerable = value as IEnumerable;

            var hasElements = valueAsEnumerable?.GetEnumerator().MoveNext() ?? false;
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            if (parameter as string == "Reverse")
            {
                hasElements = !hasElements;
            }

            return hasElements ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}