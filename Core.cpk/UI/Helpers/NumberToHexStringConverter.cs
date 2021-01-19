namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class NumberToHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var number = System.Convert.ToUInt64(value);
            var digits = GetDigitsCount(value);

            return number.ToString("X" + digits);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string ?? string.Empty;
            if (!int.TryParse(str, NumberStyles.HexNumber, culture, out var result))
            {
                result = 0;
            }

            return System.Convert.ChangeType(result, targetType);
        }

        private static int GetDigitsCount(object value)
        {
            int digits;
            switch (value)
            {
                default:
                case byte:
                case sbyte:
                    digits = 1;
                    break;

                case short:
                case ushort:
                    digits = 2;
                    break;

                case int:
                case uint:
                    digits = 4;
                    break;

                case long:
                case ulong:
                    digits = 8;
                    break;
            }

            return digits;
        }
    }
}