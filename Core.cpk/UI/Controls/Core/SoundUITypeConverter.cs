namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class SoundUITypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(
            ITypeDescriptorContext context,
            Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(
            ITypeDescriptorContext context,
            Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            return new SoundUI() { Path = (string)value };
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            return ((SoundUI)value).Path;
        }
    }
}