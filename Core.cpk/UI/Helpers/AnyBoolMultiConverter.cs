namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class AnyBoolMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isReversed = parameter as string == "Reverse";

            foreach (var o in values)
            {
                if (!(o is bool b))
                {
                    throw new Exception("Not a boolean value: " + o);
                }

                if (isReversed)
                {
                    b = !b;
                }

                if (b)
                {
                    return true;
                }
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}