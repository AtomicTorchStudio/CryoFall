namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class ValueToPercentsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (System.Convert.ToDouble(value) * 100).ToString("F0") + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}