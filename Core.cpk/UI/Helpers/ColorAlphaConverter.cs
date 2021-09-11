namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Extensions;

    public class ColorAlphaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Color color)
            {
                throw new Exception("Not a color");
            }

            if (parameter is byte alpha)
            {
                return color.WithAlpha(alpha);
            }

            if (parameter is string str)
            {
                str = str.Trim();
                if (str.IndexOf("0x", StringComparison.Ordinal) == 0)
                {
                    str = str.Substring(2);
                }

                if (byte.TryParse(str,
                                  NumberStyles.HexNumber,
                                  provider: null,
                                  out alpha))
                {
                    return color.WithAlpha(alpha);
                }
            }

            throw new Exception("The alpha parameter is not correct. It should be a byte value.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}