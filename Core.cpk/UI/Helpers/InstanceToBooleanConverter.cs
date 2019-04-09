namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class InstanceToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true.Equals(value)
                       ? parameter
                       : Binding.DoNothing;
        }
    }
}