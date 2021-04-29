namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class AllBoolMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isReversed = parameter as string == "Reverse";

            foreach (var o in values)
            {
                var b = System.Convert.ToBoolean(o);
                if (isReversed)
                {
                    b = !b;
                }

                if (!b)
                {
                    return false;
                }
            }

            return true;
        }

        public object[] ConvertBack(object values, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}