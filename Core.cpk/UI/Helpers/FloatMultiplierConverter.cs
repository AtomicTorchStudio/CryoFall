namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class FloatMultiplierConverter : IValueConverter
    {
        public static readonly FloatMultiplierConverter Instance = new FloatMultiplierConverter();

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var number = System.Convert.ToDouble(value);

            double multiplier;
            try
            {
                multiplier = System.Convert.ToDouble(parameter);
            }
            catch
            {
                Api.Logger.Error(
                    "Please ensure that the ConverterParameter contains the multiplier (a floating point value)");
                multiplier = 1.0;
            }

            return System.Convert.ChangeType(number * multiplier, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}