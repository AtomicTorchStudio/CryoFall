namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    public class ColorMultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Color color))
            {
                throw new Exception("Not a color");
            }

            var multiplier = System.Convert.ToDouble(parameter);
            return Color.FromArgb(color.A,
                                  (byte)(color.R * multiplier),
                                  (byte)(color.G * multiplier),
                                  (byte)(color.B * multiplier));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}