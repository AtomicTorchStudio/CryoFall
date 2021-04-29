namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class NumberEqualityToVisibilityConverter : IValueConverter
    {
        public virtual bool IsReverse => false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ReSharper disable once PossibleNullReferenceException
            var valueAsNumber = System.Convert.ToInt64(value);
            var parameterAsNumber = System.Convert.ToInt64(parameter);

            var result = valueAsNumber == parameterAsNumber;
            if (this.IsReverse)
            {
                result = !result;
            }

            return result
                       ? Visibility.Visible
                       : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}