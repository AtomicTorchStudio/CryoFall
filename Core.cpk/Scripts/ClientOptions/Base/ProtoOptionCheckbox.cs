namespace AtomicTorch.CBND.CoreMod.ClientOptions
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public abstract class ProtoOptionCheckbox<TProtoOptionsCategory>
        : ProtoOption<TProtoOptionsCategory, bool>
        where TProtoOptionsCategory : ProtoOptionsCategory, new()
    {
        public override void RegisterValueType(IClientStorage storage)
        {
            // System.Double is already registered
        }

        protected override void CreateControlInternal(
            out FrameworkElement labelControl,
            out FrameworkElement optionControl)
        {
            labelControl = new FormattedTextBlock()
            {
                Content = this.Name,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 9, 0, 0)
            };

            var checkbox = new CheckBox()
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };

            this.SetupOptionToControlValueBinding(checkbox, ToggleButton.IsCheckedProperty);
            optionControl = checkbox;
        }
    }
}