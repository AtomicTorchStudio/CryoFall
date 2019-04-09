namespace AtomicTorch.CBND.CoreMod.ClientOptions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public abstract class ProtoOptionCombobox
        <TProtoOptionsCategory,
         TEnumValue>
        : ProtoOption<
            TProtoOptionsCategory,
            TEnumValue>
        where TProtoOptionsCategory : ProtoOptionsCategory, new()
        where TEnumValue : struct, Enum
    {
        private readonly Lazy<Dictionary<TEnumValue, ViewModelEnum<TEnumValue>>> viewModels;

        public ProtoOptionCombobox()
        {
            this.viewModels =
                new Lazy<Dictionary<TEnumValue, ViewModelEnum<TEnumValue>>>(
                    () => this.GetItemsViewModels().ToDictionary(p => p.Value, p => p));
        }

        public sealed override TEnumValue Default
            => this.DefaultEnumValue;

        public abstract TEnumValue DefaultEnumValue { get; }

        //// TODO: remove this bad override as soon as NoesisGUI will support value binding for combobox
        //public override void ApplyAbstractValue(object value)
        //{
        //    if (value is TEnumValue casted
        //        && this.viewModels.Value.TryGetValue(casted, out var valueViewModel))
        //    {
        //        base.ApplyAbstractValue(valueViewModel);
        //        return;
        //    }

        //    Logger.WriteWarning(
        //        $"Option {this.Name} cannot apply abstract value - type mismatch. Will reset option to the default value");
        //    this.Reset();
        //}

        public override object GetAbstractValue()
        {
            return this.CurrentValue;
        }

        public virtual ViewModelEnum<TEnumValue>[] GetItemsViewModels()
        {
            return EnumHelper.EnumValuesToViewModel<TEnumValue>();
        }

        public override void RegisterValueType(IClientStorage storage)
        {
            storage.RegisterType(typeof(TEnumValue));
        }

        protected override TEnumValue ClampValue(TEnumValue value)
        {
            if (Enum.IsDefined(typeof(TEnumValue), value))
            {
                return value;
            }

            return this.DefaultEnumValue;
        }

        protected override void CreateControlInternal(
            out FrameworkElement labelControl,
            out FrameworkElement optionControl)
        {
            labelControl  = new FormattedTextBlock()
            {
                Content = this.Name,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 7, 0, 0)
            };

            var combobox = new ComboBox()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var viewModelsDictionary = this.viewModels.Value;
            combobox.ItemsSource = viewModelsDictionary.Values.ToArray();
            combobox.DisplayMemberPath = "Description";
            // TODO: rewrite this as soon as NoesisGUI will support value binding for combobox
            // see http://bugs.noesisengine.com/view.php?id=1132
            //combobox.SelectedValuePath = "Value"; // and use Selector.SelectedValueProperty
            //this.SetupOptionToControlValueBinding(combobox, Selector.SelectedItemProperty);

            // use custom binding with converter
            combobox.SetBinding(
                Selector.SelectedItemProperty,
                new Binding("Value")
                {
                    Converter = new ConverterEnumToViewModel(viewModelsDictionary)
                });
            combobox.DataContext = this.InternalOptionValueHolder;

            optionControl = combobox;
        }

        private class ConverterEnumToViewModel : IValueConverter
        {
            private readonly Dictionary<TEnumValue, ViewModelEnum<TEnumValue>> dictionary;

            public ConverterEnumToViewModel(Dictionary<TEnumValue, ViewModelEnum<TEnumValue>> dictionary)
            {
                this.dictionary = dictionary;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var casted = Enum.ToObject(typeof(TEnumValue), value);
                return this.dictionary[(TEnumValue)casted];
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return ((ViewModelEnum<TEnumValue>)value).Value;
            }
        }
    }
}